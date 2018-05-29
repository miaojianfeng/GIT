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
}