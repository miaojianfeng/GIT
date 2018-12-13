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
using System.IO;
using System.Xml.Linq;

namespace EMPower
{
    // ---------- UI Binding Source ----------
    public class EMPowerParams : INotifyPropertyChanged
    {
        // ---------- Field ----------
        private static object locker = new object();  
        
        // ---------- Constructor ---------- 
        public EMPowerParams()           
        {

        }

        // ---------- Field ----------
        private int comPortIndex = 0;
        private int filterIndex = 0;
        private bool isConnected = false;
        private bool isComPortsExist = false;
        private string firmwareVer = string.Empty;
        private string connStaMsg = "未连接";
        private string errMsg = string.Empty;
        private string pwrResult = string.Empty;

        // ---------- Property ----------
        public bool IsComPortsExist
        {
            set
            {
                lock(locker)
                {
                    this.isComPortsExist = value;
                    NotifyPropertyChanged("IsComPortsExist");
                }
                
            }
            get
            {
                lock (locker)
                {
                    return this.isComPortsExist;
                }                
            }
        }

        public bool IsConnected
        {
            set
            {
                lock (locker)
                {
                    this.isConnected = value;
                    NotifyPropertyChanged("IsConnected");
                }                
            }
            get
            {
                lock (locker)
                {
                    return this.isConnected;
                }                
            }
        }               

        public string FirmwareVersion
        {
            set
            {
                lock (locker)
                {
                    this.firmwareVer = value;
                    NotifyPropertyChanged("FirmwareVersion");
                }                
            }
            get
            {
                lock (locker)
                {
                    return this.firmwareVer;
                }                
            }
        }

        public string ConnectStatusMessage
        {
            set
            {
                lock (locker)
                {
                    this.connStaMsg = value;
                    NotifyPropertyChanged("ConnectStatusMessage");
                }                
            }
            get
            {
                lock (locker)
                {
                    return this.connStaMsg;
                }                
            }
        }

        public string ErrorMessage
        {
            set
            {
                lock (locker)
                {
                    this.errMsg = value;
                    NotifyPropertyChanged("ErrorMessage");
                }                
            }
            get
            {
                lock (locker)
                {
                    return this.errMsg;
                }                
            }
        }

        public string PowerResult
        {
            set
            {
                lock(locker)
                {
                    this.pwrResult = value;
                    NotifyPropertyChanged("PowerResult");
                }
            }
            get
            {
                lock(locker)
                {
                    return this.pwrResult;
                }
            }
        }

        public int FilterIndex
        {
            set
            {
                lock(locker)
                {
                    this.filterIndex = value;
                    NotifyPropertyChanged("FilterIndex");
                }
            }
            get
            {
                lock (locker)
                {
                    return this.filterIndex;
                }
            }
        }

        public int ComPortIndex
        {
            set
            {
                lock(locker)
                {
                    this.comPortIndex = value;
                    NotifyPropertyChanged("ComPortIndex");
                }
            }
            get
            {
                lock(locker)
                {
                    return this.comPortIndex;
                }
            }
        }

        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------
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
    }

    public class TboxTextToButtonEnabledConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            if(text!=string.Empty)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class ConnectStateToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isConnected = (bool)value;
            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);

            GradientStopCollection redLED = new GradientStopCollection() { new GradientStop(Colors.Pink, 0),
                                                                           new GradientStop(Colors.Red, 0.5),
                                                                           new GradientStop(Colors.DarkRed, 1)};

            // DarkGreen LED
            //GradientStopCollection darkGreenLED = new GradientStopCollection() { new GradientStop(Colors.DarkGreen, 0),
            //                                                                     new GradientStop(Colors.Green, 0.75),
            //                                                                     new GradientStop(Colors.LimeGreen, 0.85),
            //                                                                     new GradientStop(Colors.LightGreen, 0.9),
            //                                                                     new GradientStop(Colors.LightGreen, 1)};

            // LightGreen LED
            GradientStopCollection lightGreenLED = new GradientStopCollection() { new GradientStop(Colors.White, 0),
                                                                                  new GradientStop(Colors.LightGreen, 0.35),
                                                                                  new GradientStop(Colors.LimeGreen, 0.85),
                                                                                  new GradientStop(Colors.Green, 0.9),
                                                                                  new GradientStop(Colors.DarkGreen, 1)};

            if(isConnected)
            {
                brush.GradientStops = new GradientStopCollection(lightGreenLED);
            }
            else
            {                
                brush.GradientStops = new GradientStopCollection(redLED);
            }

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class ConnStaMsgToFontColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            BrushConverter brushConverter = new BrushConverter();

            switch (text)
            {
                case "连接中...":                    
                    return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Yellow");                    
                case "未连接":
                    return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Red");                    
                case "已连接":
                    return (System.Windows.Media.Brush)brushConverter.ConvertFromString("DarkGreen");
                default:
                    return (System.Windows.Media.Brush)brushConverter.ConvertFromString("Black");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class ComboxSelectedIndexToButtonEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            if (index!=-1 && index!=0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class InstConnStaToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isConnected = (bool)value;

            LinearGradientBrush brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(0, 1);

            // RedButton
            GradientStopCollection redButton = new GradientStopCollection() { new GradientStop(Colors.Pink, 0),
                                                                           new GradientStop(Colors.Red, 0.5),
                                                                           new GradientStop(Colors.DarkRed, 1)};

            // GreenButton
            GradientStopCollection greenButton = new GradientStopCollection() { new GradientStop(Colors.LightGreen, 0),
                                                                                new GradientStop(Colors.Green, 0.9),
                                                                                new GradientStop(Colors.DarkGreen, 1)};
            
            if(isConnected)
            {
                brush.GradientStops = new GradientStopCollection(redButton);
            }
            else
            {
                brush.GradientStops = new GradientStopCollection(greenButton);
            }
            
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class BoolReverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (bool)value;
            return !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }
}