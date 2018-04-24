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
            Timeout_ms = SimParams.Timeout_ms;
            IsAutoNotifyMode = SimParams.IsAutoNotifyMode;
            IsDoor1Closed = SimParams.IsDoor1Closed;
            IsDoor2Closed = SimParams.IsDoor2Closed;

            // DirtyFlag
            DirtyFlag = false;

            // Instance Trace Window 
            this.traceWnd = new TraceWindow();
            this.traceWnd.CloseTraceWnd = CloseTraceWindow;
            HideTraceWnd();
        }

        // Property
        public ModbusTcpSocketServer TcpSvr { get; private set; }
        public ZL6042DISimulator ZL6042Sim { get; private set; }
        public DoorSimulatorParams SimParams { get; private set; }
        
        private bool IsAutoNotifyMode { set; get; }
        private bool IsDoor1Closed { set; get; }
        private bool IsDoor2Closed { set; get; }
        private int Timeout_ms { set; get; }        
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
                if (Timeout_ms != SimParams.Timeout_ms || TcpSvr.ServerPort != SimParams.PortNum || IsAutoNotifyMode!= SimParams.IsAutoNotifyMode)
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
            if(TcpSvr.ServerState!=EnumServerState.ServerStopped)
            {
                TcpSvr.Stop();
            }

            IsAutoNotifyMode = SimParams.IsAutoNotifyMode;
            Timeout_ms = SimParams.Timeout_ms;
            TcpSvr.ServerPort = SimParams.PortNum;
            TcpSvr.QueryTimeout_ms = SimParams.Timeout_ms;
            this.expdr.IsExpanded = false;
            DirtyFlag = false;

            e.Handled = true;
        }

        // EventHandler
        private void RadioBtnDoor1State_Changed(object sender, RoutedEventArgs e)
        {
            if(SimParams!=null)
            {
                if (IsDoor1Closed != SimParams.IsDoor1Closed)
                {
                    IsDoor1Closed = SimParams.IsDoor1Closed;
                    if (IsDoor1Closed)
                    {
                        ZL6042Sim.DIStateCh1 = EnumDIState.HighLevel;
                    }
                    else
                    {
                        ZL6042Sim.DIStateCh1 = EnumDIState.LowLevel;
                    }
                }
            }
        }

        private void RadioBtnDoor2State_Changed(object sender, RoutedEventArgs e)
        {
            if (SimParams != null)
            {
                if (IsDoor2Closed != SimParams.IsDoor2Closed)
                {
                    IsDoor2Closed = SimParams.IsDoor2Closed;
                    if (IsDoor2Closed)
                    {
                        ZL6042Sim.DIStateCh2 = EnumDIState.HighLevel;
                    }
                    else
                    {
                        ZL6042Sim.DIStateCh2 = EnumDIState.LowLevel;
                    }
                }
            }
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            TcpSvr.ProcessMessage = ZL6042Sim.ProcessDIStateQueryMessage;
            TcpSvr.EnableTrace = true;
            TcpSvr.UpdateTrace = this.traceWnd.UpdateTrace; // Update Trace
            
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
                SimParams.Timeout_ms = Timeout_ms;
                SimParams.PortNum = TcpSvr.ServerPort;
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
    }
}
