using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PositionerControl
{
    public class PositionerCtrlCmds
    {
        // ------------ Field ------------
        // Query Position Commands
        private static RoutedUICommand queryPos_Slide;
        private static RoutedUICommand queryPos_Lift;
        private static RoutedUICommand queryPos_Turntable;

        // Seek Position Commands
        private static RoutedUICommand seekAbsolutePos_Slide;
        private static RoutedUICommand seekAbsolutePos_Lift;
        private static RoutedUICommand seekAbsolutePos_Turntable;

        // Seek Relative Position Commands
        private static RoutedUICommand seekRelativePos_Slide;
        private static RoutedUICommand seekRelativePos_Lift;
        private static RoutedUICommand seekRelativePos_Turntable;

        // Stop Moving Commands
        private static RoutedUICommand stop_Slide;
        private static RoutedUICommand stop_Lift;
        private static RoutedUICommand stop_Turntable;

        // Set Settings Commands
        private static RoutedUICommand applySettings_Slide;
        private static RoutedUICommand applySettings_Lift;
        private static RoutedUICommand applySettings_Turntable;

        // ---------- Constructor ----------
        static PositionerCtrlCmds()
        {
            // Position querying commands            
            queryPos_Slide = new RoutedUICommand("Query Position for Slide", "QueryPos_Slide", typeof(PositionerCtrlCmds));
            queryPos_Lift = new RoutedUICommand("Query Position for Lift", "QueryPos_Lift", typeof(PositionerCtrlCmds));
            queryPos_Turntable = new RoutedUICommand("Query Position for Turntable", "QueryPos_Turntable", typeof(PositionerCtrlCmds));

            // Absolute position querying commands
            seekAbsolutePos_Slide = new RoutedUICommand("Seek Absolute Position for Slide", "SeekAbsPos_Slide", typeof(PositionerCtrlCmds));
            seekAbsolutePos_Lift = new RoutedUICommand("Seek Absolute Position for Lift", "SeekAbsPos_Lift", typeof(PositionerCtrlCmds));
            seekAbsolutePos_Turntable = new RoutedUICommand("Seek Absolute Position for Turntable", "SeekAbsPos_Turntable", typeof(PositionerCtrlCmds));

            // Relative position querying commands
            seekRelativePos_Slide = new RoutedUICommand("Seek Relative Position for Slide", "SeekRelPos_Slide", typeof(PositionerCtrlCmds));
            seekRelativePos_Lift = new RoutedUICommand("Seek Relative Position for Lift", "SeekRelPos_Lift", typeof(PositionerCtrlCmds));
            seekRelativePos_Turntable = new RoutedUICommand("Seek Relative Position for Turntable", "SeekRelPos_Turntable", typeof(PositionerCtrlCmds));

            // Stop commands
            stop_Slide = new RoutedUICommand("Stop moving for Slide", "Stop_Slide", typeof(PositionerCtrlCmds));
            stop_Lift = new RoutedUICommand("Stop moving for Lift", "Stop_Lift", typeof(PositionerCtrlCmds));
            stop_Turntable = new RoutedUICommand("Stop moving for Turntable", "Stop_Turntable", typeof(PositionerCtrlCmds));

            // Set Settings commands
            applySettings_Slide = new RoutedUICommand("Apply Settings for Slide", "ApplySettings_Slide", typeof(PositionerCtrlCmds));
            applySettings_Lift = new RoutedUICommand("Apply Settings for Lift", "ApplySettings_Lift", typeof(PositionerCtrlCmds));
            applySettings_Turntable = new RoutedUICommand("Apply Settings for Turntable", "ApplySettings_Turntable", typeof(PositionerCtrlCmds));
        }

        // ------------ Property ------------
        // Position Querying Commands
        public static RoutedUICommand QueryPos_Slide
        {
            get
            {
                return queryPos_Slide;
            }
        }
        public static RoutedUICommand QueryPos_Lift
        {
            get
            {
                return queryPos_Lift;
            }
        }
        public static RoutedUICommand QueryPos_Turntable
        {
            get
            {
                return queryPos_Turntable;
            }
        }

        // Seek Absolute Position Commands
        public static RoutedUICommand SeekAbsolutePos_Slide
        {
            get
            {
                return seekAbsolutePos_Slide;
            }
        }
        public static RoutedUICommand SeekAbsolutePos_Lift
        {
            get
            {
                return seekAbsolutePos_Lift;
            }
        }
        public static RoutedUICommand SeekAbsolutePos_Turntable
        {
            get
            {
                return seekAbsolutePos_Turntable;
            }
        }

        // Seek Relative Position Commands
        public static RoutedUICommand SeekRelativePos_Slide
        {
            get
            {
                return seekRelativePos_Slide;
            }
        }
        public static RoutedUICommand SeekRelativePos_Lift
        {
            get
            {
                return seekRelativePos_Lift;
            }
        }
        public static RoutedUICommand SeekRelativePos_Turntable
        {
            get
            {
                return seekRelativePos_Turntable;
            }
        }

        // Stop Moving Commands
        public static RoutedUICommand Stop_Slide
        {
            get
            {
                return stop_Slide;
            }
        }
        public static RoutedUICommand Stop_Lift
        {
            get
            {
                return stop_Lift;
            }
        }
        public static RoutedUICommand Stop_Turntable
        {
            get
            {
                return stop_Turntable;
            }
        }

        // Set Settings commands
        public static RoutedUICommand ApplySettings_Slide
        {
            get
            {
                return applySettings_Slide;
            }
        }
        public static RoutedUICommand ApplySettings_Lift
        {
            get
            {
                return applySettings_Lift;
            }
        }
        public static RoutedUICommand ApplySettings_Turntable
        {
            get
            {
                return applySettings_Turntable;
            }
        }
    }
}
