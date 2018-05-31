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

namespace DoorMonitor
{
    public class DoorStatusToFormColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumDoorStatus isDoorOpen = (EnumDoorStatus)value;
            BrushConverter brushConverter = new BrushConverter();

            if (isDoorOpen==EnumDoorStatus.Open)
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Red");
            }
            else if(isDoorOpen == EnumDoorStatus.Closed)
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Green");
            }
            else
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Gray");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class DoorStatusToFontColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumDoorStatus isDoorOpen = (EnumDoorStatus)value;
            BrushConverter brushConverter = new BrushConverter();

            if (isDoorOpen == EnumDoorStatus.Open)
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Yellow");
            }
            else if (isDoorOpen == EnumDoorStatus.Closed)
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Blue");
            }
            else
            {
                return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Black");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class Door1StatusToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumDoorStatus isDoorOpen = (EnumDoorStatus)value;
            BrushConverter brushConverter = new BrushConverter();

            if (isDoorOpen == EnumDoorStatus.Open)
            {
                return "Door1 Open!";
            }
            else if (isDoorOpen == EnumDoorStatus.Closed)
            {
                return "Door1 Closed";
            }
            else
            {
                return "Door1 Ignored";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class Door2StatusToMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumDoorStatus isDoorOpen = (EnumDoorStatus)value;
            BrushConverter brushConverter = new BrushConverter();

            if (isDoorOpen == EnumDoorStatus.Open)
            {
                return "Door2 Open!";
            }
            else if (isDoorOpen == EnumDoorStatus.Closed)
            {
                return "Door2 Closed";
            }
            else
            {
                return "Door2 Ignored";
            }
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

    public class ZLAN6042LinkStateToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EnumZLAN6042LinkStatus state = (EnumZLAN6042LinkStatus)value;

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);

            // RedLED
            GradientStopCollection redLED = new GradientStopCollection() { new GradientStop(Colors.Pink, 0),
                                                                           new GradientStop(Colors.Red, 0.5),
                                                                           new GradientStop(Colors.DarkRed, 1)};
            

            // LightGreen LED
            GradientStopCollection lightGreenLED = new GradientStopCollection() { new GradientStop(Colors.White, 0),
                                                                                  new GradientStop(Colors.LightGreen, 0.35),
                                                                                  new GradientStop(Colors.LimeGreen, 0.85),
                                                                                  new GradientStop(Colors.Green, 0.9),
                                                                                  new GradientStop(Colors.DarkGreen, 1)};

            switch (state)
            {
                case EnumZLAN6042LinkStatus.Disconnected:
                    brush.GradientStops = new GradientStopCollection(redLED);
                    break;
                case EnumZLAN6042LinkStatus.Connected:
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
            
            // RedLED
            GradientStopCollection redLED = new GradientStopCollection() { new GradientStop(Colors.Pink, 0),
                                                                           new GradientStop(Colors.Red, 0.5),
                                                                           new GradientStop(Colors.DarkRed, 1)};
            
            // LightGreen LED
            GradientStopCollection lightGreenLED = new GradientStopCollection() { new GradientStop(Colors.White, 0),
                                                                                  new GradientStop(Colors.LightGreen, 0.35),
                                                                                  new GradientStop(Colors.LimeGreen, 0.85),
                                                                                  new GradientStop(Colors.Green, 0.9),
                                                                                  new GradientStop(Colors.DarkGreen, 1)};

            switch (state)
            {
                case EnumMsgTransState.Silence:
                    brush.GradientStops = new GradientStopCollection(redLED);
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
}