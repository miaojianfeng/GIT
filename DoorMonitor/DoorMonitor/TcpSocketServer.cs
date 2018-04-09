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
using System.Windows.Forms;

namespace DoorMonitor
{
    public enum EnumServerState
    {
        ServerStopped   = 0,
        ServerStarted   = 1,     
        ClientConnected = 2             
    }

    public class TcpSocketServer: INotifyPropertyChanged
    {
        // ---------- Type Definition ----------
        // ---------- Field ----------       
        private TcpListener   tcpListener;       
        
        private string serverName = "TCP Server";
        private UInt16 serverPort = 8001;
        private EnumServerState serverState = EnumServerState.ServerStopped;

        private StringBuilder traceRecord = new StringBuilder();
        private int queryTimeout_ms = 200;

        static private int clientNum = 0;

        // ---------- Constructor ---------- 
        public TcpSocketServer()
        {  
            
        }

        public TcpSocketServer(string svrName, UInt16 svrPort, Action<string> svrTraceHandler = null, Func<string, string> svrMsgHandler = null)            
        {
            ServerName = svrName;
            ServerPort = svrPort;
            UpdateTrace = svrTraceHandler;
            ProcessMessage = svrMsgHandler;            
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

        public string TraceRecord
        {            
            get
            {
                return this.traceRecord.ToString();
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

        private void AppendTrace(string message)
        {
            this.traceRecord.Append(message);
            NotifyPropertyChanged("TraceRecord");

            if (UpdateTrace != null)
            {
                UpdateTrace(message);
            }
        }

        protected string ProcessRecMessage(string msgReceived)
        {
            string resp = string.Format("No Response");
            if (ProcessMessage != null)
            {
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
                AppendTrace(string.Format("{0} (localhost::{1}) started.\n", ServerName, ServerPort));

                while (true)
                {
                    clientNum++;
                    try
                    {
                        TcpClient newClient = await tcpListener.AcceptTcpClientAsync();
                        AppendTrace(String.Format("Client{0} has conected...\n", clientNum));

                        ServerState = EnumServerState.ClientConnected;                        
                        ReceiveFromClientTask(newClient, clientNum);                        
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
                AppendTrace(string.Format("{0} (localhost::{1}) stopped.\n", ServerName, ServerPort));
            }
        }

        private void ReceiveFromClientTask(TcpClient client, int num)
        {
            Task.Run(() => ReceiveFromClient(client, num));
        }
        
        private void ReceiveFromClient(TcpClient client, int num)
        {
            while (true)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    byte[] bytesReceived = new byte[1024];
                    int i;
                    StringBuilder traceTextLine = new StringBuilder();

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
                        AppendTrace(String.Format("[Client{0} ==> {1}]:  {2}", num, ServerName, traceTextLine.ToString()));

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
                            string response = ProcessRecMessage(temp3);

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

                        Thread.Sleep(QueryTimeout_ms);

                        // Send back a response.
                        if (responseArray.ToString().ToUpper() != string.Empty)
                        {
                            byte[] bytesSend = System.Text.Encoding.ASCII.GetBytes(responseArray.ToString());
                            stream.Write(bytesSend, 0, bytesSend.Length);
                            AppendTrace(String.Format("[Client{0} <== {1}]:  {2}", num, ServerName, responseArray.ToString()));
                        }
                    }
                }
                catch
                {
                    AppendTrace(String.Format("Client{0} has lost connection with {1}\n", num, ServerName));                  
                    return;
                }
            }            
        }
    }
}
