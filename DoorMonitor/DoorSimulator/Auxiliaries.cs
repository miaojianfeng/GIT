using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;
using System.Collections.ObjectModel; 
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using ETSL.TcpSocket;

namespace DoorSimulator
{
    public class DoorSimulatorParams: INotifyPropertyChanged
    {

        // Field        
        private int timeout_ms = 200;
        private UInt16 portNum = 9001;
        private bool isAutoNotify = true;
        private bool isDIDetHighToLow = true;
        private bool isDoor1Closed = true;
        private bool isDoor2Closed = true;        

        public DoorSimulatorParams()
        {

        }

        public int Timeout_ms
        {
            set
            {
                this.timeout_ms = value;
                NotifyPropertyChanged("Timeout_ms");
            }
            get
            {
                return this.timeout_ms;
            }
        }

        public UInt16 PortNum
        {
            set
            {
                this.portNum = value;
                NotifyPropertyChanged("PortNum");
            }
            get
            {
                return this.portNum;
            }
        }

        public bool IsAutoNotifyMode
        {
            set
            {
                this.isAutoNotify = value;
                NotifyPropertyChanged("IsAutoNotifyMode");
            }
            get
            {
                return this.isAutoNotify;
            }
        }

        public bool IsDIDetHighToLow
        {
            set
            {
                this.isDIDetHighToLow = value;
                NotifyPropertyChanged("IsDIDetHighToLow");
            }
            get
            {
                return this.isDIDetHighToLow;
            }
        }

        public bool IsDoor1Closed
        {
            set
            {
                this.isDoor1Closed = value;
                NotifyPropertyChanged("IsDoor1Closed");
            }
            get
            {
                return this.isDoor1Closed;
            }
        }

        public bool IsDoor2Closed
        {
            set
            {
                this.isDoor2Closed = value;
                NotifyPropertyChanged("IsDoor2Closed");
            }
            get
            {
                return this.isDoor2Closed;
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
        #endregion
    }

    public class ContraAutoNotifyStaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }
    }

    public class ContraDIDetHighToLowConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }
    }

    public class ContraDoorClosedStaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool state = (bool)value;
            return !state;
        }
    }

    public class SvrStateToRunSvrEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumServerState state = (EnumServerState)value;
            bool retVal = false;

            switch (state)
            {
                case EnumServerState.ServerStopped:
                    retVal = true;
                    break;
                case EnumServerState.ServerStarted:
                case EnumServerState.ClientConnected:
                    retVal = false;
                    break;
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class SvrStateToStopSvrEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumServerState state = (EnumServerState)value;
            bool retVal = false;

            switch (state)
            {
                case EnumServerState.ServerStopped:
                    retVal = false;
                    break;
                case EnumServerState.ServerStarted:
                case EnumServerState.ClientConnected:
                    retVal = true;
                    break;
            }
            return retVal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class SvrStateToFontColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumServerState state = (EnumServerState)value;
            BrushConverter brushConverter = new BrushConverter();
            Brush brush = null;

            switch (state)
            {
                case EnumServerState.ServerStopped:
                    brush = (System.Windows.Media.Brush)brushConverter.ConvertFromString("Red");
                    break;
                case EnumServerState.ServerStarted:
                    brush = (System.Windows.Media.Brush)brushConverter.ConvertFromString("DarkGreen");
                    break;
                case EnumServerState.ClientConnected:
                    brush = (System.Windows.Media.Brush)brushConverter.ConvertFromString("YellowGreen");
                    break;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class SvrStateToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumServerState state = (EnumServerState)value;

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);

            // RedLED
            GradientStopCollection redLED = new GradientStopCollection() { new GradientStop(Colors.Pink, 0),
                                                                           new GradientStop(Colors.Red, 0.5),
                                                                           new GradientStop(Colors.DarkRed, 1)};

            // DarkGreen LED
            GradientStopCollection darkGreenLED = new GradientStopCollection() { new GradientStop(Colors.DarkGreen, 0),
                                                                                 new GradientStop(Colors.Green, 0.75),
                                                                                 new GradientStop(Colors.LimeGreen, 0.85),
                                                                                 new GradientStop(Colors.LightGreen, 0.9),
                                                                                 new GradientStop(Colors.LightGreen, 1)};

            // LightGreen LED
            GradientStopCollection lightGreenLED = new GradientStopCollection() { new GradientStop(Colors.White, 0),
                                                                                  new GradientStop(Colors.LightGreen, 0.35),
                                                                                  new GradientStop(Colors.LimeGreen, 0.85),
                                                                                  new GradientStop(Colors.Green, 0.9),
                                                                                  new GradientStop(Colors.DarkGreen, 1)};

            switch (state)
            {
                case EnumServerState.ServerStopped:
                    brush.GradientStops = new GradientStopCollection(redLED);
                    break;
                case EnumServerState.ServerStarted:
                    brush.GradientStops = new GradientStopCollection(darkGreenLED);
                    break;
                case EnumServerState.ClientConnected:
                    brush.GradientStops = new GradientStopCollection(lightGreenLED);
                    break;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class MsgTransStateToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumMsgTransState state = (EnumMsgTransState)value;

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);

            // DarkGreen LED
            GradientStopCollection darkGreenLED = new GradientStopCollection() { new GradientStop(Colors.DarkGreen, 0),
                                                                                 new GradientStop(Colors.Green, 0.75),
                                                                                 new GradientStop(Colors.LimeGreen, 0.85),
                                                                                 new GradientStop(Colors.LightGreen, 0.9),
                                                                                 new GradientStop(Colors.LightGreen, 1)};

            // LightGreen LED
            GradientStopCollection lightGreenLED = new GradientStopCollection() { new GradientStop(Colors.White, 0),
                                                                                  new GradientStop(Colors.LightGreen, 0.35),
                                                                                  new GradientStop(Colors.LimeGreen, 0.85),
                                                                                  new GradientStop(Colors.Green, 0.9),
                                                                                  new GradientStop(Colors.DarkGreen, 1)};

            switch (state)
            {
                case EnumMsgTransState.Silence:
                    brush.GradientStops = new GradientStopCollection(darkGreenLED);
                    break;
                case EnumMsgTransState.Working:
                    brush.GradientStops = new GradientStopCollection(lightGreenLED);
                    break;
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class ExpdrStateToExpdrTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isExpanded = (bool)value;
            string text = string.Empty;

            if(isExpanded)
            {
                text = "Press here to switch back...";
            }
            else
            {
                text = "Press here to show more settings...";
            }

            return text;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class StrLenToClrTraceBtnEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = (string)value;
            if (str != string.Empty) return true;
            else return false;             
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }
}