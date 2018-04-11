using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;


namespace ETSL.Utilities
{
    public partial class NotifyIconWrapper : Component
    {
        // Use just one instance of this window
        private MainWindow wnd = new MainWindow(); // 

        public NotifyIconWrapper()
        {
            
            InitializeComponent();

            // Attach event handlers.
        }

        private void ShowWnd()
        {
            // Show the window (and bring it to the forefront if it's already visible)
            if (wnd.WindowState == System.Windows.WindowState.Minimized)
            {
                wnd.WindowState = System.Windows.WindowState.Normal;
            }

            wnd.Show();
            wnd.Activate();
        }

        private void CmdShowWindow_Click(object sender, EventArgs e)
        {
            ShowWnd();
        }

        private void CmdClose_Click(object sender, EventArgs e)
        {
            wnd.ClosePositionInfoFile();
            System.Windows.Application.Current.Shutdown();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ShowWnd();            
        }
    }
}
