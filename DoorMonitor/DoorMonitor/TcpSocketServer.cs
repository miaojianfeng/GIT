using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows.Forms;

namespace DoorMonitor
{
    public enum EnumTcpSocketServerState
    {
        Stopped = 0,
        Listening = 1,     
        Connected = 2             
    }

    public class TcpSocketServerException : Exception
    {
#pragma warning disable 1591
        public TcpSocketServerException() { }
        public TcpSocketServerException(string message) : base(message) { }
        public TcpSocketServerException(string message, Exception inner) : base(message, inner) { }
        protected TcpSocketServerException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
#pragma warning restore 1591
    }

    public interface ITcpSocketServer
    {
        // ---------- Property ----------
        string ServerName { get; set; }

        UInt16 ServerPort { get; set; }

        string ConfigFile { get; set; }

        Action<string> TraceHandler { get; set; }

        Func<string, string> CommandHandler { get; set; }

        EnumTcpSocketServerState State { get; }

        // ----------- Method ----------               
        void Start();

        void Stop();
    }


    public class TcpSocketServer: INotifyPropertyChanged, ITcpSocketServer
    {
        // ---------- Type Definition ----------
        // ---------- Field ----------       
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private CancellationTokenSource cts;
        //private Dictionary<string, string> dictCmdVsResp;  // Store the "Command vs Response" Table

        private StringBuilder lastError = new StringBuilder(200);
        private string serverName = string.Empty;
        private UInt16 serverPort = 8001;
        private string configFile = string.Empty;        
        private StringBuilder traces = new StringBuilder();
        private EnumTcpSocketServerState stateMachine = EnumTcpSocketServerState.Stopped;
        private int queryInterval_ms = 0;

        // ---------- Constructor ---------- 
        public TcpSocketServer()
        {
            this.InitMembers("TcpSocketServer", 2000);
        }


        public TcpSocketServer(string svrName, UInt16 svrPort, string svrConfigFile = "", Action<string> svrTraceHandler = null, Func<string, string> svrCmdHandler = null)
        {
            this.InitMembers(svrName, svrPort, svrConfigFile, svrTraceHandler, svrCmdHandler);
        }

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
        public string ConfigFile
        {
            set
            {
                this.configFile = value;
                NotifyPropertyChanged("ConfigFile");
            }
            get
            {
                return this.configFile;
            }
        }

        public EnumTcpSocketServerState State
        {
            private set
            {
                this.stateMachine = value;
                NotifyPropertyChanged("State");
            }
            get
            {
                return this.stateMachine;
            }
        }

        public string LastError
        {
            private set
            {
                this.lastError.Clear();
                this.lastError.Append(value);
                NotifyPropertyChanged("LastError");
            }
            get
            {
                return this.lastError.ToString();
            }
        }

        public int QueryInterval_ms
        {
            set
            {
                this.queryInterval_ms = value;
                NotifyPropertyChanged("QueryInterval_ms");
            }
            get
            {
                return this.queryInterval_ms;
            }
        }

        public string Traces
        {
            private set
            {
                this.traces.Append(value);
                NotifyPropertyChanged("Traces");
            }
            get
            {
                return this.traces.ToString();
            }
        }

        public Action<string> TraceHandler { get; set; }

        public Func<string, string> CommandHandler { get; set; }

        public void ClearTraces()
        {
            this.traces.Clear();
            NotifyPropertyChanged("Traces");
        }        

        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------
        private void InitMembers(string svrName,
                                 UInt16 svrPort,
                                 string svrConfigFile = "",
                                 Action<string> svrTraceHandler = null,
                                 Func<string, string> svrCmdHandler = null)
        {
            // Fields            
            this.tcpClient = null;
            this.tcpListener = null;
            this.cts = new CancellationTokenSource();
            this.traces.Clear();

            // Properties
            LastError = string.Empty;
            ServerName = svrName;            
            ServerPort = svrPort;            
            ConfigFile = svrConfigFile;   
            State = EnumTcpSocketServerState.Stopped;
            TraceHandler = svrTraceHandler;      // Default: null;
            CommandHandler = svrCmdHandler;  //Default: null;
            QueryInterval_ms = 100;
        }
        
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

        virtual public void Trace(string message)
        {
            Traces = message;

            if (TraceHandler != null)
            {
                TraceHandler(message);
            }
        }

        protected string ProcessCommand(string cmdReceived)
        {
            string resp = string.Format("No Response");
            if (CommandHandler != null)
            {
                resp = CommandHandler(cmdReceived);
            }
            
            return resp;
        }

        // ------------------------------ State Machine Description ----------------------------------
        // <1> [Idle] State:                 IsSessionCreate==false && IsListeningLoopRunning==false;
        // <2> [TCP  :                       IsSessionCreate==true && IsListeningLoopRunning==false;
        // <3> [TCP Session Created] State:  IsSessionCreate==true && IsListeningLoopRunning==true;
        // -------------------------------------------------------------------------------------------

        public async void Start()
        {
            if (State == EnumTcpSocketServerState.Stopped)
            {
                this.ClearTraces();
                await CreateTcpSessionAsync();
                
                try
                {
                    Task task = new Task(RunTcpListeningLoop);
                    task.Start();
                }
                catch
                {
                    Stop();
                }               
            }
        }

        public void Stop()
        {
            if (State != EnumTcpSocketServerState.Stopped)
            {
                Trace("/// TCP socket server stops.\n");
                this.tcpListener.Stop();
                this.tcpListener = null;
                this.tcpClient = null;
                State = EnumTcpSocketServerState.Stopped;                   
            }
        }

        private async Task CreateTcpSessionAsync()
        {
            Trace(string.Format("/// {0} [Address: localhost(127.0.0.1)::{1}]\n", ServerName, ServerPort));
            this.tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), ServerPort);
            //this.tcpListener = new TcpListener(IPAddress.Parse("localhost"), ServerPort);

            this.tcpListener.Start();
            Trace("/// TCP socket server starts, waiting for connection from client...\n");
            State = EnumTcpSocketServerState.Listening;

            try
            {
                this.tcpClient = await this.tcpListener.AcceptTcpClientAsync();
                Trace("/// TCP client has connected!\n\n");
                State = EnumTcpSocketServerState.Connected;
            }
            catch
            {
                Stop();
            }
        }

        private void RunTcpListeningLoop()
        {
            Trace("------------------- Message recording starts -------------------\n");

            #region TcplisteningLoop
            while (true)
            {  
                NetworkStream stream = null;

                try
                {   
                    if (this.tcpClient != null)
                    {
                        stream = this.tcpClient.GetStream();
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    Trace(string.Format("Exception occurs: {0}\n", LastError));
                    Stop();
                                                            
                    break;
                }

                byte[] bytesReceived = new byte[1024];
                int i;
                StringBuilder traceTextLine = new StringBuilder();

                #region Receiving/Sending loop
                try
                {
                    while ((i = stream.Read(bytesReceived, 0, bytesReceived.Length)) != 0)
                    {
                        string cmdReceived = System.Text.Encoding.ASCII.GetString(bytesReceived, 0, i);
                        traceTextLine.Clear();
                        if (cmdReceived.EndsWith("\n"))
                        {
                            traceTextLine.Append(cmdReceived);
                        }
                        else
                        {
                            traceTextLine.Append(cmdReceived + "\n");
                        }
                        Trace(String.Format("[Client ==> {0}]:  {1}", ServerName, traceTextLine.ToString()));

                        // Process the received message
                        // Remove the \r\n, as well as the ";"   
                        string temp1 = cmdReceived.TrimEnd();
                        string temp2 = string.Empty;
                        if (temp1.EndsWith(";"))
                        {
                            temp2 = temp1.Remove(cmdReceived.LastIndexOf(";"), 1);
                        }
                        else
                        {
                            temp2 = temp1;
                        }

                        string[] commands = temp2.Split(new string[] { ";" }, StringSplitOptions.None);
                        int count = 0;
                        StringBuilder responseArray = new StringBuilder();
                        foreach (string command in commands)
                        {
                            count += 1;
                            // Process the data sent by the client.
                            string temp3 = command.TrimEnd();
                            string response = ProcessCommand(temp3);

                            if (count == commands.Length)
                            {
                                responseArray.Append(response);
                            }
                            else
                            {
                                responseArray.Append(response + ";");
                            }
                        }

                        if (!string.IsNullOrEmpty(responseArray.ToString()))
                        {
                            if (!responseArray.ToString().EndsWith("\n"))
                            {
                                responseArray.Append("\n");
                            }
                        }

                        Thread.Sleep(QueryInterval_ms);

                        // Send back a response.
                        if (responseArray.ToString().ToUpper() != string.Empty)
                        {
                            byte[] bytesSend = System.Text.Encoding.ASCII.GetBytes(responseArray.ToString());
                            stream.Write(bytesSend, 0, bytesSend.Length);
                            Trace(String.Format("[Client <== {0}]:  {1}", ServerName, responseArray.ToString()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    LastError = ex.Message;
                    this.Stop();

                    break;
                }
                #endregion
                if (stream != null)
                {
                    stream.Close();
                }
            }
            #endregion

            Trace("------------------- Message recording stops ---------------------\n");
        }
    }
}
