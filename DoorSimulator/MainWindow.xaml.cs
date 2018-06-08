using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices; 
using ETSL.TcpSocket;
using ETSL.Utilities;

namespace DoorSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Field
        private TraceWindow traceWnd;
        private bool isTraceWndOpened = false;

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // ModbusTcpSocketServer
            TcpSvr = (ModbusTcpSocketServer)this.FindResource("tcpSvr");
            TcpSvr.ServerPort = 9001;

            // ZL6042Simulator
            ZL6042Sim = (ZL6042DISimulator)this.FindResource("zl6042Sim");

            // Simulator Settings
            SimParams = (DoorSimulatorParams)this.TryFindResource("simParams");
            
            IsDIDetLowForOpen = SimParams.IsDIDetLowForOpen;
            IsDoor1Open = SimParams.IsDoor1Open;
            IsDoor2Open = SimParams.IsDoor2Open;

            // DirtyFlag
            DirtyFlag = false;

            // Instance Trace Window 
            this.traceWnd = new TraceWindow();
            this.traceWnd.CloseTraceWnd = CloseTraceWindow;
            HideTraceWnd();

            // Register event handler for ModbusTcpSocketServer.DIChangedEvent
            TcpSvr.DIChangedEvent += DIChangedEventHandler;
        }

        // Property
        public ModbusTcpSocketServer TcpSvr { get; private set; }
        public ZL6042DISimulator ZL6042Sim { get; private set; }
        public DoorSimulatorParams SimParams { get; private set; }

        //private bool IsAutoNotifyMode { set; get; }
        private bool IsDIDetLowForOpen { set; get; }
        private bool IsDoor1Open { set; get; }
        private bool IsDoor2Open { set; get; }
        //private int Timeout_ms { set; get; }        
        private bool DirtyFlag { set; get; }

        private bool IsTraceWndOpened
        {
            set
            {
                this.isTraceWndOpened = value;
                NotifyPropertyChanged("IsTraceWndOpened");
            }
            get
            {
                return this.isTraceWndOpened;
            }
        }
        
        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------
        #region Method
        /// <summary>
        /// This method is called by the Set accessor of each property. 
        /// The CallerMemberName attribute that is applied to the optional propertyName 
        /// parameter causes the property name of the caller to be substituted as an argument.
        /// </summary>
        /// <param name="propertyName"></param>
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //public string ProcessReceivingMessage(string recString)
        //{
            
        //}

        public void ShowTraceWnd()
        {
            this.traceWnd.Show();
            this.traceWnd.WindowState = WindowState.Normal;
            IsTraceWndOpened = true;
        }

        public void HideTraceWnd()
        {
            this.traceWnd.Hide();
            IsTraceWndOpened = false;
        }

        private void CloseTraceWindow()
        {
            if (IsTraceWndOpened)
            {
                HideTraceWnd();
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
            if (!message.EndsWith("\n"))
            {
                message += "\n";
            }

            string traceText = timeStamp + " " + typeStr + "   " + message;

            traceWnd.UpdateTrace(traceText);
        }
        #endregion 

        // Commands
        private void SimSettingOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(SimParams==null || TcpSvr==null)
            {
                e.CanExecute = false;
            }
            else
            {
                if (TcpSvr.QueryTimeout_ms != SimParams.Timeout_ms || 
                    TcpSvr.ServerPort != SimParams.PortNum         || 
                    TcpSvr.IsAutoNotifyMode!= SimParams.IsAutoNotifyMode)
                {
                    DirtyFlag = true;
                    e.CanExecute = true;
                }
                else
                {
                    DirtyFlag = false;
                    e.CanExecute = false;
                }                   
            }
            e.Handled = true;
        }

        private void SimSettingOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Stop the server first
            if(TcpSvr.ServerState!=EnumServerState.ServerStopped)
            {
                MessageBox.Show( this.wndMain,
                                 "The server will be stopped before the new setting to take effect",
                                 "Information", 
                                 MessageBoxButton.OK, 
                                 MessageBoxImage.Information);
                TcpSvr.Stop();
            }

            // QueryTimeout_ms
            if (TcpSvr.QueryTimeout_ms != SimParams.Timeout_ms)
            {
                TcpSvr.QueryTimeout_ms = SimParams.Timeout_ms;
                AppendTrace(EnumTraceType.Information, string.Format("Set QueryTimeout_ms: {0}\n", TcpSvr.QueryTimeout_ms));
            }

            // ServerPort
            if(TcpSvr.ServerPort != SimParams.PortNum)
            {
                TcpSvr.ServerPort = SimParams.PortNum;
                AppendTrace(EnumTraceType.Information, string.Format("Set ServerPort: {0}\n", TcpSvr.ServerPort));
            }

            // IsAutoNotifyMode
            if(TcpSvr.IsAutoNotifyMode!=SimParams.IsAutoNotifyMode)
            {
                TcpSvr.IsAutoNotifyMode = SimParams.IsAutoNotifyMode;
                AppendTrace(EnumTraceType.Information, string.Format("Set IsAutoNotifyMode: {0}\n", TcpSvr.IsAutoNotifyMode));
                
                if (!TcpSvr.IsAutoNotifyMode)
                {                   
                    
                    if(IsDoor1Open)
                    {
                        ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                    }
                    else
                    {
                        ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                    }

                    if (IsDoor2Open)
                    {
                        ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                    }
                    else
                    {
                        ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                    }

                    ZL6042Sim.DIStateCh3 = EnumDIState.HighLevel;
                    ZL6042Sim.DIStateCh4 = EnumDIState.HighLevel;
                }
            } 

            this.expdr.IsExpanded = false;
            DirtyFlag = false;            

            e.Handled = true;
        }

        // EventHandler
        private void RadioBtnDoor1State_Changed(object sender, RoutedEventArgs e)
        {
            if(SimParams!=null)
            {
                if (IsDoor1Open != SimParams.IsDoor1Open)
                {
                    IsDoor1Open = SimParams.IsDoor1Open;                    
                                     
                    if (IsDoor1Open)
                    {
                        if(TcpSvr.IsAutoNotifyMode)
                        {
                            EnumDIStateChange diStateChange = IsDIDetLowForOpen ? EnumDIStateChange.HighToLow : EnumDIStateChange.LowToHigh;
                            string msg = ZL6042Sim.GetDIStateChangeAutoNotifyMessage(1, diStateChange);
                            TcpSvr.AutoNotificationMessage = msg;
                            TcpSvr.IsDIChanged = true;
                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Closed ==> Set DI1 to: {0}\n", IsDIDetLowForOpen ? "High" : "Low"));
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;                            

                            if (IsDoor2Open)
                            {
                                ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                            }
                            else
                            {
                                ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            }

                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Closed ==> Set DI1 to: {0}\n", IsDIDetLowForOpen ? "High" : "Low"));
                        }
                    }
                    else
                    {
                        if (TcpSvr.IsAutoNotifyMode)
                        {
                            EnumDIStateChange diStateChange = IsDIDetLowForOpen ? EnumDIStateChange.LowToHigh : EnumDIStateChange.HighToLow;
                            string msg = ZL6042Sim.GetDIStateChangeAutoNotifyMessage(1, diStateChange);
                            TcpSvr.AutoNotificationMessage = msg;
                            TcpSvr.IsDIChanged = true;
                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Open ==> Set DI1 to: {0}\n", IsDIDetLowForOpen ? "Low" : "High"));
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            
                            if (IsDoor2Open)
                            {
                                ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                            }
                            else
                            {
                                ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            }

                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Open ==> Set DI1 to: {0}\n", IsDIDetLowForOpen ? "Low" : "High"));
                        }
                    }
                }
            }
        }

        private void RadioBtnDoor2State_Changed(object sender, RoutedEventArgs e)
        {
            if (SimParams != null)
            {
                if (IsDoor2Open != SimParams.IsDoor2Open)
                {
                    IsDoor2Open = SimParams.IsDoor2Open;                    

                    if (IsDoor2Open)
                    {
                        if (TcpSvr.IsAutoNotifyMode)
                        {
                            EnumDIStateChange diStateChange = IsDIDetLowForOpen ? EnumDIStateChange.HighToLow : EnumDIStateChange.LowToHigh;
                            string msg = ZL6042Sim.GetDIStateChangeAutoNotifyMessage(2, diStateChange);                            
                            TcpSvr.AutoNotificationMessage = msg;
                            TcpSvr.IsDIChanged = true;
                            AppendTrace(EnumTraceType.Information, string.Format("Door2 Closed ==> Set DI2 to: {0}\n", IsDIDetLowForOpen ? "High" : "Low"));
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                            
                            if (IsDoor1Open)
                            {
                                ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                            }
                            else
                            {
                                ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            }

                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Closed ==> Set DI2 to: {0}\n", IsDIDetLowForOpen ? "High" : "Low"));
                        }
                    }
                    else
                    {
                        if (TcpSvr.IsAutoNotifyMode)
                        {
                            EnumDIStateChange diStateChange = IsDIDetLowForOpen ? EnumDIStateChange.LowToHigh : EnumDIStateChange.HighToLow;
                            string msg = ZL6042Sim.GetDIStateChangeAutoNotifyMessage(2, diStateChange);
                            TcpSvr.AutoNotificationMessage = msg;
                            TcpSvr.IsDIChanged = true;
                            AppendTrace(EnumTraceType.Information, string.Format("Door2 Open ==> Set DI2 to: {0}\n", IsDIDetLowForOpen ? "Low" : "High"));
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            
                            if (IsDoor1Open)
                            {
                                ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                            }
                            else
                            {
                                ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                            }

                            AppendTrace(EnumTraceType.Information, string.Format("Door1 Closed ==> Set DI2 to: {0}\n", IsDIDetLowForOpen ? "Low" : "High"));
                        }
                    }
                }
            }
        }

        private void RadioBtnDIDetect_Changed(object sender, RoutedEventArgs e)
        {
            if (SimParams != null)
            {
                if(IsDIDetLowForOpen!=SimParams.IsDIDetLowForOpen)
                {
                    IsDIDetLowForOpen = SimParams.IsDIDetLowForOpen;
                    AppendTrace(EnumTraceType.Information, string.Format("Set DI for door open detection to: {0}\n", IsDIDetLowForOpen ? "Low" : "High"));     
                    
                    if(!TcpSvr.IsAutoNotifyMode)
                    {
                        if (IsDoor1Open)
                        {
                            ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh1 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                        }

                        if (IsDoor2Open)
                        {
                            ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.LowLevel : EnumDIState.HighLevel;
                        }
                        else
                        {
                            ZL6042Sim.DIStateCh2 = IsDIDetLowForOpen ? EnumDIState.HighLevel : EnumDIState.LowLevel;
                        }
                    }          
                }
            }
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {            
            TcpSvr.EnableTrace = true;
            TcpSvr.UpdateTrace = this.traceWnd.UpdateTrace; // Update Trace
            TcpSvr.ProcessMessage = ZL6042Sim.ProcessDIStateQueryMessage;

            await TcpSvr.Start();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {  
            TcpSvr.Stop();            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.traceWnd.DestroyWndFlag = true;
            this.traceWnd.Close();

            if (TcpSvr.ServerState!=EnumServerState.ServerStopped)
            {
                TcpSvr.Stop();                
            }                     
        }

        private void expdr_Collapsed(object sender, RoutedEventArgs e)
        {
            this.expdrText.Content = "Press here to show more settings...";
            if (DirtyFlag)
            {               
                SimParams.Timeout_ms = TcpSvr.QueryTimeout_ms;
                SimParams.PortNum = TcpSvr.ServerPort;
                SimParams.IsAutoNotifyMode = TcpSvr.IsAutoNotifyMode;
                DirtyFlag = false;
            }
        }

        private void expdr_Expanded(object sender, RoutedEventArgs e)
        {
            this.expdrText.Content = "Press here to switch back...";            
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {            
            ShowTraceWnd();
        }

        private void DIChangedEventHandler(object sender, EventArgs e)
        {
            traceWnd.UpdateTrace("Detect DI Changed!");
        }
    }
}
