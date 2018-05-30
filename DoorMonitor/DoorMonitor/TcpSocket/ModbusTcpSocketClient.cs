using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using ETSL.Utilities;

namespace ETSL.TcpSocket
{
    public enum EnumDoorStatus
    {
        Ignore = -1, 
        Closed =  0, 
        Open   =  1
    }

    public class ModbusTcpSocketClient: INotifyPropertyChanged
    {
        // ----- Constructor -----
        public ModbusTcpSocketClient()
        {
           
        }

        public ModbusTcpSocketClient(string ipAddress, UInt16 portNum)
        {
            this.ipAddr = ipAddress;
            this.port = portNum;
        }

        // ----- Field -----
        private TcpClient tcpClient;
        private string ipAddr = "192.168.0.200";
        private UInt16 port = 502;
        private bool monitorDoor1 = true;
        private bool monitorDoor2 = true;
        private EnumDoorStatus isDoor1Open = EnumDoorStatus.Ignore;
        private EnumDoorStatus isDoor2Open = EnumDoorStatus.Ignore;
        private EnumMsgTransState msgTransState = EnumMsgTransState.Silence;
        static private object locker = new object();
        private StringBuilder traceRecord = new StringBuilder();
        private const string diQueryMsg = "00 00 00 00 00 06 01 01 00 00 00 04";  // DI querying message

        // ----- Property -----
        public string IPAddress
        {
            set
            {
                this.ipAddr = value;
                NotifyPropertyChanged("IPAddress");
            }
            get
            {
                return this.ipAddr;
            }
        }

        public UInt16 Port
        {
            set
            {
                this.port = value;
                NotifyPropertyChanged("Port");
            }
            get
            {
                return this.port;
            }
        }

        public bool MonitorDoor1
        {
            set
            {
                this.monitorDoor1 = value;
                NotifyPropertyChanged("MonitorDoor1");
                AppendTrace(EnumTraceType.Information, string.Format("MonitorDoor1: <{0}>", value));
            }
            get
            {
                return this.monitorDoor1;
            }
        }

        public bool MonitorDoor2
        {
            set
            {
                this.monitorDoor2 = value;
                NotifyPropertyChanged("MonitorDoor2");
                AppendTrace(EnumTraceType.Information, string.Format("MonitorDoor2: <{0}>", value));
            }
            get
            {
                return this.monitorDoor2;
            }
        }

        public EnumDoorStatus IsDoor1Open
        {
            set
            {
                lock (locker)
                {
                    this.isDoor1Open = value;
                    NotifyPropertyChanged("IsDoor1Open");
                    AppendTrace(EnumTraceType.Information, string.Format("IsDoor1Open: <{0}>", value.ToString()));
                } 
            }
            get
            {
                return this.isDoor1Open;
            }
        }

        public EnumDoorStatus IsDoor2Open
        {
            set
            {
                lock (locker)
                {
                    this.isDoor2Open = value;
                    NotifyPropertyChanged("IsDoor2Open");
                    AppendTrace(EnumTraceType.Information, string.Format("IsDoor2Open: <{0}>", value.ToString()));
                }
            }
            get
            {
                return this.isDoor2Open;
            }
        }

        public EnumMsgTransState MsgTransState
        {
            private set
            {
                this.msgTransState = value;
                NotifyPropertyChanged("MsgTransState");
            }
            get
            {
                return this.msgTransState;
            }
        }

        public Action<string> UpdateTrace { get; set; }

        public Action ShowAlertMessage { get; set; }
        public Action ShowMainWindow { get; set; }

        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------
        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument.
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void StartMonitor()
        {
            try
            {
                this.tcpClient = new TcpClient(IPAddress, Port);
                AppendTrace(EnumTraceType.Information, string.Format("Connect to ZLAN6042({0}::{1}) successfully!", IPAddress, Port));
            }
            catch
            {
                AppendTrace(EnumTraceType.Exception, string.Format("Connect to ZLAN6042({0}::{1}) failed!", IPAddress, Port));
                
                this.tcpClient = null;
                this.UpdateTrace = null;
                MessageBox.Show("Failed to connect to ZLAN6042!\nPlease check network connection.", "Information");
                return;
            }

            CheckDoorStatus(this.tcpClient);
            ReceiveFromClientTask(this.tcpClient);           
        }

        public void StopMonitor()
        {
            if (this.tcpClient != null)
            {
                this.tcpClient.Close();
                this.tcpClient = null;
                this.UpdateTrace = null;
            }
        }

        private void ReceiveFromClientTask(TcpClient client)
        {
            Task.Run(() => ReceiveFromServer(client));
        }

        private void ReceiveFromServer(TcpClient client)
        {
            NetworkStream nwkStream = client.GetStream();
            byte[] bytesReceived = new byte[1024];
            int i;
            StringBuilder recMsg = new StringBuilder();
            //StringBuilder sendMsg = new StringBuilder();

            //// Tricky skill here: 
            //// BinaryReader is used for the purpose of detecting whether client has disconnected.           
            BinaryReader br = new BinaryReader(nwkStream);

            while (true)
            {
                try
                {
                    // To use NetworkStream to read/write message
                    #region NetworkStream Read/Write   
                    MsgTransState = EnumMsgTransState.Silence;

                    // Listening loop for DI automatical notification 
                    while ((i = nwkStream.Read(bytesReceived, 0, bytesReceived.Length)) != 0)                    
                    {
                        MsgTransState = EnumMsgTransState.Working;

                        recMsg.Clear();
                        for (int j = 0; j < i; j++)
                        {
                            recMsg.Append(bytesReceived[j].ToString("X2"));

                            if (j != i - 1) recMsg.Append(" ");
                        }

                        AppendTrace(EnumTraceType.Message, String.Format("PC <== ZLAN6042 :  {0}", recMsg.ToString().ToUpper()));

                        // Process received message
                        ProcessDiAutoNotificationMsg(recMsg.ToString());

                        MsgTransState = EnumMsgTransState.Silence;
                                               
                    }
                    #endregion

                    // To use BinaryReader to detect whether client is still connected
                    // If client is disconnected, BinaryReader.ReaderString will throw an exception
                    // One important point is: this "br.ReadString()" must be put after NetworkStream Read/Write while loop
                    // Otherwise the message sent from client cannot be readout
                    br.ReadString();
                }
                catch
                {
                    nwkStream.Close();
                    MsgTransState = EnumMsgTransState.Silence;
                    AppendTrace(EnumTraceType.Information, "PC has disconnected\n");
                    return;
                }
            }
        }

        protected void CheckDiStatus(string msgReceived)
        {
            string[] msgArray = msgReceived.Split(new string[] { " " }, StringSplitOptions.None);

            if (msgArray.Length == 10)
            {
                // Convert text to nunmber
                Int16 diValue = Convert.ToInt16(msgArray[9]);
                Int16 DI1 = 0x1;
                Int16 DI2 = 0x2;
                //Int16 DI3 = 0x4;
                //Int16 DI4 = 0x8;

                // DI1
                if (MonitorDoor1)
                {
                    if ((diValue & DI1) == DI1)  // Closed           
                    {
                        IsDoor1Open = EnumDoorStatus.Closed;                        
                    }
                    else  // Open
                    {
                        IsDoor1Open = EnumDoorStatus.Open;                        
                    }
                }
                else
                {
                    IsDoor1Open = EnumDoorStatus.Ignore;                    
                }

                // DI2
                if (MonitorDoor2)
                {
                    if ((diValue & DI2) == DI2)  // Closed           
                    {
                        IsDoor2Open = EnumDoorStatus.Closed;                        
                    }
                    else  // Open
                    {
                        IsDoor2Open = EnumDoorStatus.Open;                        
                    }
                }
                else
                {
                    IsDoor2Open = EnumDoorStatus.Ignore;
                }  
                
                if(IsDoor1Open==EnumDoorStatus.Open || IsDoor2Open==EnumDoorStatus.Open)
                {
                    ShowAlertMessage();
                }       
            }
        }

        private void CheckDoorStatus(TcpClient client)
        {
            NetworkStream nwkStream = client.GetStream();            
            StringBuilder sendMsg = new StringBuilder();
            StringBuilder recMsg = new StringBuilder();
            byte[] bytesReceived = new byte[1024];            

            try
            {
                MsgTransState = EnumMsgTransState.Silence;

                sendMsg.Clear();
                sendMsg.Append(diQueryMsg);
                byte[] bytesSend = Utilities.Auxiliaries.strToToHexByte(sendMsg.ToString());
                nwkStream.Write(bytesSend, 0, bytesSend.Length);
                AppendTrace(EnumTraceType.Message, String.Format("PC ==> ZLAN6042:  {0}", sendMsg.ToString().ToUpper()));
                MsgTransState = EnumMsgTransState.Working;

                System.Threading.Thread.Sleep(200);

                MsgTransState = EnumMsgTransState.Silence;
                int i = nwkStream.Read(bytesReceived, 0, bytesReceived.Length);                
                
                recMsg.Clear();
                for (int j = 0; j < i; j++)
                {
                    recMsg.Append(bytesReceived[j].ToString("X2"));

                    if (j != i - 1) recMsg.Append(" ");
                }
                AppendTrace(EnumTraceType.Message, String.Format("PC <== ZLAN6042 :  {0}", recMsg.ToString().ToUpper()));
                MsgTransState = EnumMsgTransState.Working;

                // Process received message
                CheckDiStatus(recMsg.ToString());

                //nwkStream.Close();
                MsgTransState = EnumMsgTransState.Silence;
            }
            catch
            {
                nwkStream.Close();
                MsgTransState = EnumMsgTransState.Silence;                
                AppendTrace(EnumTraceType.Information, String.Format("PC has disconnected\n"));
                return;
            }

            MsgTransState = EnumMsgTransState.Silence;
        }        

        protected void ProcessDiAutoNotificationMsg(string msgReceived)
        {
            string[] msgArray = msgReceived.Split(new string[] { " " }, StringSplitOptions.None);
            if(msgArray.Length==12)
            {
                if (MonitorDoor1)
                {
                    if (msgArray[9].ToUpper() == "10") // DI1:10 / DI2:11 / DI3:12 / DI4:13
                    {
                        if (msgArray[10].ToUpper() == "FF")   // Door Closed
                        {
                            IsDoor1Open = EnumDoorStatus.Closed;
                        }

                        if (msgArray[10].ToUpper() == "00")  // Door Open
                        {
                            IsDoor1Open = EnumDoorStatus.Open;
                            ShowAlertMessage();
                            System.Threading.Thread.Sleep(1000);
                            ShowMainWindow();
                        }
                    }
                }
                else
                {
                    IsDoor1Open = EnumDoorStatus.Ignore;
                }

                if (MonitorDoor2)
                {
                    if (msgArray[9].ToUpper() == "11") // DI1:10 / DI2:11 / DI3:12 / DI4:13
                    {
                        if (msgArray[10].ToUpper() == "FF")   // Door Closed
                        {
                            IsDoor2Open = EnumDoorStatus.Closed;
                        }

                        if (msgArray[10].ToUpper() == "00")  // Door Open
                        {
                            IsDoor2Open = EnumDoorStatus.Open;
                            ShowAlertMessage();
                            System.Threading.Thread.Sleep(1000);
                            ShowMainWindow();
                        }
                    }
                }
                else
                {
                    IsDoor2Open = EnumDoorStatus.Ignore;
                }                
            }            
        }
        
        private void AppendTrace(EnumTraceType traceType, string message)
        {
            // Add time stamp in the beginning of the trace record
            string timeStamp = "[ " + Auxiliaries.TimeStampGenerate() + " ]";

            // Trace type
            string typeStr = string.Empty;
            switch (traceType)
            {
                case EnumTraceType.Information:
                    typeStr = "[ INFO ]";
                    break;
                case EnumTraceType.Error:
                    typeStr = "[ ERR ]";
                    break;
                case EnumTraceType.Exception:
                    typeStr = "[ EXCEPTION ]";
                    break;
                case EnumTraceType.Message:
                    typeStr = "[ MSG ]";
                    break;
            }

            // Trace body
            if (!message.EndsWith("\n"))
            {
                message += "\n";
            }

            string traceText = timeStamp + " " + typeStr + "   " + message;

            // Multiple threads may manipulate the same target concurrently 
            lock (locker)
            {
                this.traceRecord.Append(traceText);
                NotifyPropertyChanged("TraceRecord");

                if (UpdateTrace != null)
                {
                    UpdateTrace(traceText);
                }
            }
        }
    }
}
