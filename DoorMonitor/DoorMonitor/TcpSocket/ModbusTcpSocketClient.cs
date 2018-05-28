using System;
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
        private bool isDoor1Open = false;
        private bool isDoor2Open = false;
        private EnumMsgTransState msgTransState = EnumMsgTransState.Silence;
        static private object locker = new object();
        private StringBuilder traceRecord = new StringBuilder();

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

        public bool IsDoor1Open
        {
            set
            {
                this.isDoor1Open = value;
                NotifyPropertyChanged("IsDoo1Open");
            }
        }

        public bool IsDoor2Open
        {
            set
            {
                this.isDoor2Open = value;
                NotifyPropertyChanged("IsDoo2Open");
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
                AppendTrace(EnumTraceType.Information, string.Format("Connected with ZL6042({0}::{1}) successfully!", IPAddress, Port));
            }
            catch
            {
                AppendTrace(EnumTraceType.Exception, string.Format("Connected with ZL6042({0}::{1}) failed!", IPAddress, Port));
                return;
            }

            ReceiveFromClientTask(this.tcpClient);           
        }

        public void StopMonitor()
        {
            this.tcpClient.Close();
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
                        //ProcessRecMessage(recMsg.ToString());

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
                    MsgTransState = EnumMsgTransState.Silence;                    
                    AppendTrace(EnumTraceType.Information, "PC has disconnected\n");
                    return;
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

        protected void ProcessRecMessage(string msgReceived)
        { 

        }
    }
}
