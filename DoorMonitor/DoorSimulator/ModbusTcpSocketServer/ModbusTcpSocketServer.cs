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
    public enum EnumServerState
    {
        ServerStopped   = 0,
        ServerStarted   = 1,     
        ClientConnected = 2             
    }

    public enum EnumMsgTransState
    {
        Silence = 0,
        Working = 1
    }

    public enum EnumTraceType
    {
        Information = 0,
        Error = 1,
        Exception = 2,
        Message = 3
    }

    public delegate void EventHandler(Object sender, EventArgs e);       

    public class ModbusTcpSocketServer: INotifyPropertyChanged
    {
        // ---------- Type Definition ----------
        // ---------- Field ----------       
        private TcpListener   tcpListener;       
        
        private string serverName = "TCP Server";
        private UInt16 serverPort = 8001;
        private EnumServerState serverState = EnumServerState.ServerStopped;
        private EnumMsgTransState msgTransState = EnumMsgTransState.Silence;
        private bool enableTrace = false;
        private bool isDIChanged = false;

        private StringBuilder traceRecord = new StringBuilder();
        private int queryTimeout_ms = 200;

        static private int clientNum = 0;
        static private object locker = new object();

        // ---------- Constructor ---------- 
        public ModbusTcpSocketServer()
        {
            IsAutoNotifyMode = true;
            //IsDIChanged = false;
            AutoNotificationMessage = string.Empty;
        }

        public ModbusTcpSocketServer(UInt16 svrPort)
        {
            ServerPort = svrPort;
            IsAutoNotifyMode = true;
            //IsDIChanged = false;
            AutoNotificationMessage = string.Empty;
        }

        public ModbusTcpSocketServer(string svrName, UInt16 svrPort)
        {
            ServerName = svrName;
            ServerPort = svrPort;
            IsAutoNotifyMode = true;
            //IsDIChanged = false;
            AutoNotificationMessage = string.Empty;
        }

        public ModbusTcpSocketServer(string svrName, UInt16 svrPort, Action<string> svrTraceHandler = null, Func<string, string> svrMsgHandler = null)            
        {
            ServerName = svrName;
            ServerPort = svrPort;
            UpdateTrace = svrTraceHandler;
            ProcessMessage = svrMsgHandler;
            IsAutoNotifyMode = true;
            //IsDIChanged = false;
            AutoNotificationMessage = string.Empty;
        }

        // Event
        public event EventHandler DIChangedEvent; 
        
        // ---------- Property ----------
        public string ServerName
        {
            set
            {
                this.serverName = value;
                NotifyPropertyChanged("ServerName");
            }
            get
            {
                return this.serverName;
            }
        }
        public UInt16 ServerPort
        {
            set
            {
                this.serverPort = value;
                NotifyPropertyChanged("ServerPort");
            }
            get
            {
                return this.serverPort;
            }
        }
        
        public EnumServerState ServerState
        {
            private set
            {
                this.serverState = value;
                NotifyPropertyChanged("ServerState");
            }
            get
            {
                return this.serverState;
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

        public int QueryTimeout_ms
        {
            set
            {
                this.queryTimeout_ms = value;
                NotifyPropertyChanged("QueryTimeout_ms");
            }
            get
            {
                return this.queryTimeout_ms;
            }
        }

        public bool EnableTrace
        {
            set
            {
                this.enableTrace = value;
                NotifyPropertyChanged("ServerPort");
            }
            get
            {
                return this.enableTrace;
            }
        }

        public string TraceRecord
        {            
            get
            {
                return this.traceRecord.ToString();
            }
        }

        public bool IsAutoNotifyMode { set; get; }
        public string AutoNotificationMessage { set; get; }
        public bool IsDIChanged
        {
            set
            {
                lock(locker)
                {
                    this.isDIChanged = value;
                    NotifyPropertyChanged("IsDIChanged");
                }
            }
            get
            {
                lock (locker)
                {
                    return this.isDIChanged;
                }
            }
        }
        
        public Action<string> UpdateTrace { get; set; }

        public Func<string, string> ProcessMessage { get; set; }
        
        public void ClearTraceRecord()
        {
            this.traceRecord.Clear();
            NotifyPropertyChanged("MessageRecord");
        }        

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

        private void AppendTrace(EnumTraceType traceType, string message)
        {
            if (!EnableTrace) return;

            // Add time stamp in the beginning of the trace record
            string timeStamp = "[ " + Auxiliaries.TimeStampGenerate() + " ]";

            // Trace type
            string typeStr = string.Empty;
            switch(traceType)
            {
                case EnumTraceType.Information:
                    typeStr = "[ INF ]";
                    break;
                case EnumTraceType.Error:
                    typeStr = "[ ERR ]";
                    break;
                case EnumTraceType.Exception:
                    typeStr = "[ EXC ]";
                    break;
                case EnumTraceType.Message:
                    typeStr = "[ MSG ]";
                    break;
            }

            // Trace body
            if(!message.EndsWith("\n"))
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

        protected string ProcessRecMessage(string msgReceived)
        {
            string resp = string.Format("No Response");
            if (ProcessMessage != null)
            {
                msgReceived.TrimEnd();
                resp = ProcessMessage(msgReceived);
            }
            
            return resp;
        }

        public async Task Start()
        {
            if (ServerState == EnumServerState.ServerStopped)
            {
                this.tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
                this.tcpListener.Start();
                ServerState = EnumServerState.ServerStarted;
                AppendTrace(EnumTraceType.Information, string.Format("{0} (localhost::{1}) started...\n", ServerName, ServerPort));

                while (true)
                {
                    clientNum++;
                    try
                    {
                        TcpClient newClient = await tcpListener.AcceptTcpClientAsync();                        
                        AppendTrace(EnumTraceType.Information, String.Format("Client{0} has conected...\n", clientNum));

                        ServerState = EnumServerState.ClientConnected; 
                        
                        if(IsAutoNotifyMode)
                        {
                            SendNotificationMessageTask(newClient, clientNum);
                        }   
                        else
                        {
                            ReceiveFromClientTask(newClient, clientNum);
                        }                      
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }

        public void Stop()
        {
            if (ServerState != EnumServerState.ServerStopped)
            {
                tcpListener.Stop();                
                ServerState = EnumServerState.ServerStopped;
                MsgTransState = EnumMsgTransState.Silence;
                AppendTrace(EnumTraceType.Information, string.Format("{0} (localhost::{1}) stopped.\n", ServerName, ServerPort));
            }
        }

        private void ReceiveFromClientTask(TcpClient client, int num)
        {
            Task.Run(() => ReceiveFromClient(client, num));
        }
        
        private void ReceiveFromClient(TcpClient client, int num)
        {
            NetworkStream nwkStream = client.GetStream();
            byte[] bytesReceived = new byte[1024];
            int i;
            StringBuilder recMsg = new StringBuilder();
            StringBuilder sendMsg = new StringBuilder();

            // Tricky skill here: 
            // BinaryReader is used for the purpose of detecting whether client has disconnected.           
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
                        for(int j=0;j<i;j++)
                        {
                            recMsg.Append(bytesReceived[j].ToString("X2"));

                            if (j!=i-1) recMsg.Append(" ");
                        }     
                                           
                        AppendTrace(EnumTraceType.Message, String.Format("Client{0} ==> {1}:  {2}", num, ServerName, recMsg.ToString().ToUpper()));

                        // Process received message
                        sendMsg.Clear();
                        sendMsg.Append(ProcessRecMessage(recMsg.ToString()));
                          
                        Thread.Sleep(QueryTimeout_ms);

                        byte[] bytesSend = Utilities.Auxiliaries.strToToHexByte(sendMsg.ToString());
                        nwkStream.Write(bytesSend, 0, bytesSend.Length);
                        AppendTrace(EnumTraceType.Message, String.Format("Client{0} <== {1}:  {2}", num, ServerName, sendMsg.ToString().ToUpper()));

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
                    ServerState = EnumServerState.ServerStarted;
                    AppendTrace(EnumTraceType.Information, String.Format("Client{0} has disconnected\n", num, ServerName));                                     
                    return;
                }
            }            
        }

        private void SendNotificationMessageTask(TcpClient client, int num)
        {
            Task.Run(() => SendNotificationMessage(client, num));
        }

        private void SendNotificationMessage(TcpClient client, int num)
        {
            NetworkStream nwkStream = client.GetStream();
            byte[] bytesReceived = new byte[1024];
            StringBuilder sendMsg = new StringBuilder();

            while (true)
            {
                try
                {
                    MsgTransState = EnumMsgTransState.Silence;

                    if (IsDIChanged && AutoNotificationMessage!=string.Empty)
                    {
                        sendMsg.Clear();
                        sendMsg.Append(AutoNotificationMessage);
                        byte[] bytesSend = Utilities.Auxiliaries.strToToHexByte(sendMsg.ToString());
                        nwkStream.Write(bytesSend, 0, bytesSend.Length);

                        // Invoke DIChangedEvent here
                        if (DIChangedEvent != null) DIChangedEvent(this, new EventArgs());

                        AppendTrace(EnumTraceType.Message, String.Format("Client{0} <== {1}:  {2}", num, ServerName, sendMsg.ToString().ToUpper()));
                        MsgTransState = EnumMsgTransState.Working;
                        isDIChanged = false;
                    }                                
                }
                catch
                {
                    MsgTransState = EnumMsgTransState.Silence;
                    ServerState = EnumServerState.ServerStarted;
                    AppendTrace(EnumTraceType.Information, String.Format("Client{0} has disconnected\n", num));
                    return;
                }
            }
        }
    }
}
