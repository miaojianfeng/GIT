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

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpSocketServer tcpSvr;

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            tcpSvr = new TcpSocketServer("Server", 8001, UpdateTrace, ProcessCommand);
            tcpSvr.QueryTimeout_ms = 100;            
            await tcpSvr.Start();                                                                 
        }

        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            tcpSvr.Stop();
        }

        private void UpdateTrace(string trace)
        {
            this.Dispatcher.Invoke( ()=> { this.tboxTrace.AppendText(trace); } );
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
    }
}
