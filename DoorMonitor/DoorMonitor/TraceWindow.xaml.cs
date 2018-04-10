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
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Interop;


namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for TraceWindow.xaml
    /// </summary>
    public partial class TraceWindow : Window
    {
        public TraceWindow()
        {
            InitializeComponent();

            //CloseTraceWnd = null;
        }

        public Action CloseTraceWnd { set; get; }  
        
        /// <summary>
        /// Invoked by the TcpSocketServer task 
        /// </summary>
        /// <param name="trace"></param>
        public void UpdateTrace(string trace)
        {
            this.Dispatcher.Invoke(() => { this.tboxTrace.AppendText(trace); });
        }

        private void btnSaveTrace_Click(object sender, RoutedEventArgs e)
        {
            // Not implement yet
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(CloseTraceWnd!=null)
            {
                CloseTraceWnd();
            }
        }
    }
}
