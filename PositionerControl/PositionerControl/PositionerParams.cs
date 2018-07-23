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

namespace PositionerControl
{
    public enum EnumPositionerParameter
    {
        VisaAddress,
        Slide_Offset,
        Lift_Offset,
        Turntable_Offset
    }

    // Positioner Parameters
    public class PositionerParams: INotifyPropertyChanged
    {
        // Constructor
        public PositionerParams()
        {
            ConfigXML = @"C:\Temp\PositionerConfiguration.xml"; 

            // Load Configuration XML file if it exists, otherwise create it            
            if (!File.Exists(ConfigXML))
            {
                CreateConfigXML();  // create
            }

            LoadConfigXML(); // load Configuration XML file
        }

        // Field
        private string visaAddr = string.Empty;
        private double slideOffset = 0;
        private double liftOffset = 0;
        private double turntableOffset = 0;
        static private object locker = new object();

        // Property
        private string ConfigXML { set; get; }

        public string VisaAddress
        {
            get
            {
                return this.visaAddr;
            }
            set
            {
                this.visaAddr = value;
                SavePositionerParameter(EnumPositionerParameter.VisaAddress);
                NotifyPropertyChanged("VisaAddress");
            }

        }
        public double SlideOffset
        {
            get
            {
                return this.slideOffset;
            }
            set
            {
                this.slideOffset = value;
                SavePositionerParameter(EnumPositionerParameter.Slide_Offset);
                NotifyPropertyChanged("SlideOffset");
            }
        }
        public double LiftOffset
        {
            get
            {
                return this.liftOffset;
            }
            set
            {
                this.liftOffset = value;
                SavePositionerParameter(EnumPositionerParameter.Lift_Offset);
                NotifyPropertyChanged("LiftOffset");
            }
        }
        public double TurntableOffset
        {
            get
            {
                return this.turntableOffset;
            }
            set
            {
                this.turntableOffset = value;
                SavePositionerParameter(EnumPositionerParameter.Turntable_Offset);
                NotifyPropertyChanged("TurntableOffset");
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

        private void CreateConfigXML()
        {            
            try
            {
                XDocument configXmlDoc = new XDocument(new XElement("Configuration",                                                           
                                                           new XElement("VisaAddress", "TCPIP0::192.168.127.254::4001::SOCKET"),
                                                           new XElement("PositionerOffset",
                                                               new XElement("SlideOffset", "0"),
                                                               new XElement("LiftOffset", "0"),
                                                               new XElement("TurntableOffset","0"))));                
                configXmlDoc.Save(ConfigXML);
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Create <{0}> Error!\n{1}", ConfigXML, ex.Message);
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadConfigXML()
        {
            try
            {
                XDocument configXmlDoc = XDocument.Load(ConfigXML);
                XElement rootNode  = configXmlDoc.Element("Configuration");
                string addr = rootNode.Element("VisaAddress").Value;
                if(addr!=string.Empty)
                {
                    VisaAddress = addr;
                }
                else
                {
                    VisaAddress = "TCPIP0::192.168.127.254::4001::SOCKET";
                }

                string offset_Slide = rootNode.Element("PositionerOffset").Element("SlideOffset").Value;
                try
                {
                    SlideOffset = Convert.ToDouble(offset_Slide);
                }
                catch
                {
                    SlideOffset = 0;
                }

                string offset_Lift = rootNode.Element("PositionerOffset").Element("LiftOffset").Value;
                try
                {
                    LiftOffset = Convert.ToDouble(offset_Lift);
                }
                catch
                {
                    LiftOffset = 0;
                }

                string offset_TT = rootNode.Element("PositionerOffset").Element("TurntableOffset").Value;
                try
                {
                    TurntableOffset = Convert.ToDouble(offset_TT);
                }
                catch
                {
                    TurntableOffset = 0;
                }

            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Load configuration file <{0}> failed!\n{1}", ConfigXML, ex.Message);
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePositionerParameter(EnumPositionerParameter parameter)
        {            
            lock (locker)
            {
                XDocument xml = XDocument.Load(ConfigXML);
                XElement rootNode = xml.Element("Configuration");
                switch (parameter)
                {
                    case EnumPositionerParameter.VisaAddress:
                        rootNode.SetElementValue("VisaAddress", this.visaAddr);
                        break;
                    case EnumPositionerParameter.Slide_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("SlideOffset", this.slideOffset.ToString("#.##"));
                        break;
                    case EnumPositionerParameter.Lift_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("LiftOffset", this.liftOffset.ToString("#.##"));
                        break;
                    case EnumPositionerParameter.Turntable_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("TurntableOffset", this.turntableOffset.ToString("#.##"));
                        break;
                }
                xml.Save(ConfigXML);
            }            
        }
    }

    // Converter
    public class PositionerInitStateToFillColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool InitState = (bool)value;

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

            if (InitState)
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

    public class PositionerInitStateToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool InitState = (bool)value;

            if (InitState)
            {
                return new SolidColorBrush(Colors.Green);
            }
            else
            {
                return new SolidColorBrush(Colors.Red);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class PositionerInitStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool InitState = (bool)value;

            if (InitState)
            {
                return "连接成功！";
            }
            else
            {
                return "连接失败！";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class ComboxSelIndexToButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int selIndex = (int)value;

            if (selIndex==-1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }
}
