using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoorMonitor
{
    class DoorMonitorCommands
    {
        private static RoutedUICommand setParams;
        private static RoutedUICommand testCmd;

        // ---------- Constructor ----------
        static DoorMonitorCommands()
        {
            // Initialize commands            
            setParams = new RoutedUICommand("Set Parameters", "SetParameters", typeof(DoorMonitorCommands));
            testCmd = new RoutedUICommand("Test Command", "TestCommand", typeof(DoorMonitorCommands));
        }

        public static RoutedUICommand SetParameters
        {
            get
            {
                return setParams;
            }
        }

        public static RoutedUICommand TestCommand
        {
            get
            {
                return testCmd;
            }
        }
    }
}
