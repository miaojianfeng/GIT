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
using ETSL.TcpSocketServer;
using ETSL.Utilities;

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Field
        private TcpSocketServer tcpSvr;
        private TraceWindow traceWnd;
        private bool isTraceWndOpened = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Instance Trace Window 
            this.traceWnd = new TraceWindow();
            HideTraceWnd();
            this.traceWnd.CloseTraceWnd = FireChkboxUnCheckedEvent_ShowTraceWnd;        
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
            IsTraceWndOpened = false;
        }
        #endregion

        // EventHandler
        #region EventHandler
        /// <summary>
        /// btnStartServer_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            this.tcpSvr = new TcpSocketServer("Server", 8001);
            this.tcpSvr.EnableTrace = true;
            this.tcpSvr.QueryTimeout_ms = 100;
            this.tcpSvr.ProcessMessage = ProcessCommand;
            this.tcpSvr.UpdateTrace = this.traceWnd.UpdateTrace;
            await tcpSvr.Start();                                                                 
        }

        /// <summary>
        /// btnStopServer_Click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            tcpSvr.Stop();
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
        

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
        }
        #endregion
    }
}
