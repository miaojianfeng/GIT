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
using System.IO;

namespace DoorSimulator
{
    /// <summary>
    /// Interaction logic for TraceWindow.xaml
    /// </summary>
    public partial class TraceWindow : Window
    {
        public TraceWindow()
        {
            InitializeComponent();

            DestroyWndFlag = false;
        }

        static private object locker = new object();

        public Action CloseTraceWnd { set; get; }
        public bool DestroyWndFlag { set; get; }

        /// <summary>
        /// Invoked by the TcpSocket task 
        /// </summary>
        /// <param name="trace"></param>
        public void UpdateTrace(string trace)
        {
            this.Dispatcher.Invoke(() => { lock(locker) { this.tboxTrace.AppendText(trace);}});
        }

        private void btnSaveTrace_Click(object sender, RoutedEventArgs e)
        {
            string timeStamp = ETSL.Utilities.Auxiliaries.DateTimeInfoGenerate();
            string fileName = string.Format(@"c:\temp\SvrTrace_{0}.txt", timeStamp);

            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
            {
                fs.Lock(0, fs.Length);
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(this.tboxTrace.Text);
                fs.Unlock(0, fs.Length);//一定要用在Flush()方法以前，否则抛出异常。
                sw.Flush();
            }
        }

        private void btnClearTrace_Click(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.Invoke(() => { lock (locker) { this.tboxTrace.Clear(); } });
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DestroyWndFlag == false)
            {
                e.Cancel = true;

                if (CloseTraceWnd != null)
                {
                    CloseTraceWnd();
                }
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
