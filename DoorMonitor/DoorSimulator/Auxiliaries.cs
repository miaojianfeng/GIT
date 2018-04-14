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
            brush.EndPoint = new Point(0, 1);

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

}
