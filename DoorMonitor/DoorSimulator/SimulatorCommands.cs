using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DoorSimulator
{
    public static class SimulatorCommands
    {
        private static RoutedUICommand simSettingOK;        

        // ---------- Constructor ----------
        static SimulatorCommands()
        {
            // Initialize commands            
            simSettingOK = new RoutedUICommand("Simulator setting is done", "SimSettingOK", typeof(SimulatorCommands));            
        }

        public static RoutedUICommand SimSettingOK
        {
            get
            {
                return simSettingOK;
            }
        }
    }
}
