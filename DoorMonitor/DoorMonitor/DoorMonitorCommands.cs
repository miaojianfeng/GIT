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
        private static RoutedUICommand setDoorMonitorParams;
        private static RoutedUICommand testRfOffCmd;

        // ---------- Constructor ----------
        static DoorMonitorCommands()
        {
            // Initialize commands            
            setDoorMonitorParams = new RoutedUICommand("Set DoorMonitor Params", "SetDoorMonitorParams", typeof(DoorMonitorCommands));
            testRfOffCmd = new RoutedUICommand("Test RF OFF Command", "TestRfOffCommand", typeof(DoorMonitorCommands));
        }

        public static RoutedUICommand SetDoorMonitorParams
        {
            get
            {
                return setDoorMonitorParams;
            }
        }

        public static RoutedUICommand TestRfOffCommand
        {
            get
            {
                return testRfOffCmd;
            }
        }
    }
}
