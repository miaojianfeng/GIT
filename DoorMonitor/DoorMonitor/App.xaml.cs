using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public EventWaitHandle ProgramStarted { get; set; }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    //bool createNew;
        //    //ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, "DoorMonitorApp", out createNew);

        //    //if (!createNew)
        //    //{
        //    //    MessageBox.Show("Application already exit", "Information");
        //    //    App.Current.Shutdown();
        //    //    Environment.Exit(0);
        //    //}

        //    //base.OnStartup(e);

        //    //this.ShutdownMode = ShutdownMode.OnExplicitShutdown;            
        //}

        //protected override void OnExit(ExitEventArgs e)
        //{
        //    base.OnExit(e);            
        //}
    }
}
