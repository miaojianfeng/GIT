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
        // ---------- Constructor ---------- 
        public EMPowerParams()           
        {

        }

        // ---------- Property ----------
        private bool isComPortListExists = false;
        private bool isConnected = false;        
        private string firmwareVer = string.Empty;
        private string connStaMsg = "未连接";
        private string errMsg = string.Empty;

        // ---------- Property ----------
        public bool IsComPortListExists
        {
            set
            {
                this.isComPortListExists = value;
                NotifyPropertyChanged("IsComPortListExists");
            }
            get
            {
                return this.isComPortListExists;
            }
        }

        public bool IsConnected
        {
            set
            {
                this.isConnected = value;
                NotifyPropertyChanged("IsConnected");
            }
            get
            {
                return this.isConnected;
            }
        }               

        public string FirmwareVersion
        {
            set
            {
                this.firmwareVer = value;
                NotifyPropertyChanged("FirmwareVersion");
            }
            get
            {
                return this.firmwareVer;
            }
        }

        public string ConnectStatusMessage
        {
            set
            {
                this.connStaMsg = value;
                NotifyPropertyChanged("ConnectStatusMessage");
            }
            get
            {
                return this.connStaMsg;
            }
        }

        public string ErrorMessage
        {
            set
            {
                this.errMsg = value;
                NotifyPropertyChanged("ErrorMessage");
            }
            get
            {
                return this.errMsg;
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
}