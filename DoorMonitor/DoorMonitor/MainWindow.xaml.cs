using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
using System.Windows.Controls.Primitives;
using ETSL.TcpSocket;
using ETSL.Utilities;

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Field
        //private TcpSocketServer tcpSvr;
        //private ModbusTcpSocketClient modbusTcpClient; 
        private TraceWindow traceWnd;
        private bool isTraceWndOpened = false;

        private WindowState lastWindowState;

        private TcpSocketServer TcpServer { set; get; }
        private ModbusTcpSocketClient ModbusTcpClient { set; get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Instance Trace Window 
            this.traceWnd = new TraceWindow();
            this.traceWnd.CloseTraceWnd = FireChkboxUnCheckedEvent_ShowTraceWnd;
            HideTraceWnd();

            // Flag used to decide whether to destroy window or just minimize the window
            DestroyMainWnd = false;
            double left, top;        
            GetMainWndStartupPosition(out left, out top);
            this.Left = MainWndLeftPos = left;
            this.Top = MainWndTopPos = top;
            this.Show();

            // Start monitor
            TcpServer = (TcpSocketServer)this.FindResource("tcpServer");
            ModbusTcpClient = (ModbusTcpSocketClient)this.FindResource("modbusTcpClient");
            StartMonitor();
        }

        // Property
        /// <summary>
        /// IsTraceWndOpened
        /// </summary>
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

        private bool DestroyMainWnd { get; set; }

        private double MainWndLeftPos { set; get; }
        private double MainWndTopPos { set; get; }

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

        private void GetMainWndStartupPosition(out double left, out double top)
        {
            double widthWorkArea  = SystemParameters.WorkArea.Width;  // Get the work area width
            double heightWorkArea = SystemParameters.WorkArea.Height; // Get the work area Height
            left = widthWorkArea  - this.Width;
            top = heightWorkArea - this.Height;            
        }

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

        private string ProcessCommand(string command)
        {
            string respMsg = "No Response";
            switch (command.ToLower())
            {
                case "hello?":
                    respMsg = "World!";
                    break;
                default:
                    break;
            }

            return respMsg;
        }        

        public void FireChkboxUnCheckedEvent_ShowTraceWnd()
        {
            this.chkboxShowTrace.IsChecked = false;           
        }
        #endregion

        // EventHandler
        #region EventHandler    
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.lastWindowState = WindowState;
            this.Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.lastWindowState = this.WindowState;
            }
        }

        // The following two function code shows how to operate NotifyIcon behaviors
        
        //private void OnVisibilityClick(object sender, RoutedEventArgs e)
        //{
        //    this.notifyIcon.Visibility = this.notifyIcon.Visibility == Visibility.Visible ?
        //        Visibility.Collapsed : Visibility.Visible;
        //}

        
        //private void OnBalloonClick(object sender, RoutedEventArgs e)
        //{
        //    if (!string.IsNullOrEmpty(this.notifyIcon.BalloonTipText))
        //    {
        //        this.notifyIcon.ShowBalloonTip(2000);
        //    }
        //}

        private void OnNotifyIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Left = MainWndLeftPos;
                this.Top = MainWndTopPos;
                this.Show();
                this.WindowState = this.lastWindowState;
            }
        }

        // NotifyIcon Context Menu Item <Open>
        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            this.Left = MainWndLeftPos;
            this.Top = MainWndTopPos;
            this.Show();
            this.WindowState = this.lastWindowState;                       
        }

        // NotifyIcon Context Menu Item <Exit>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            DestroyMainWnd = true;
            this.Close();

            // Stop Monitor
            StopMonitor();
        }

        private async void StartMonitor()
        {
            // Modbus_TCP with ZL6042
            ModbusTcpClient.IPAddress = "192.168.0.200";
            ModbusTcpClient.Port = 502;
            ModbusTcpClient.UpdateTrace = this.traceWnd.UpdateTrace;
            ModbusTcpClient.StartMonitor();
            
            TcpServer.ServerName = "Server";
            TcpServer.ServerPort = 8001;
            TcpServer.EnableTrace = true;
            TcpServer.QueryTimeout_ms = 100;
            TcpServer.ProcessMessage = ProcessCommand;
            TcpServer.UpdateTrace = this.traceWnd.UpdateTrace;
            await TcpServer.Start();
        }
        
        private void StopMonitor()
        {
            if (ModbusTcpClient != null) ModbusTcpClient.StopMonitor();
            if (TcpServer!=null) TcpServer.Stop();            
        }

        private void chkboxShowTrace_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsTraceWndOpened)
            {
                ShowTraceWnd();
            }
        }

        private void chkboxShowTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsTraceWndOpened)
            {
                HideTraceWnd();
            }
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(DestroyMainWnd)
            {
                if (this.traceWnd != null)
                {
                    this.traceWnd.DestroyWndFlag = true;
                    this.traceWnd.Close();
                }
            }
            else
            {
                e.Cancel = true;  // Bypass window destroy procedure but just minimize the window
                WindowState = WindowState.Minimized;
            }
        }
    }
}
