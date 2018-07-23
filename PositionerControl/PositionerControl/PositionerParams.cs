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
    public enum EnumPositionerType
    {
        Slide,
        Lift,
        Turntable
    }

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
        private double offset_Slide     = 0;
        private double offset_Lift      = 0;
        private double offset_Turntable = 0;

        private bool isMovingStop_Slide     = true;
        private bool isMovingStop_Lift      = true;
        private bool isMovingStop_Turntable = true;

        private int currentSpeed_Slide = -99999;
        private int currentSpeed_Lift = -99999;
        private int currentSpeed_Turntable = -99999;

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
        public double Offset_Slide
        {
            get
            {
                return this.offset_Slide;
            }
            set
            {
                this.offset_Slide = value;
                SavePositionerParameter(EnumPositionerParameter.Slide_Offset);
                NotifyPropertyChanged("Offset_Slide");
            }
        }
        public double Offset_Lift
        {
            get
            {
                return this.offset_Lift;
            }
            set
            {
                this.offset_Lift = value;
                SavePositionerParameter(EnumPositionerParameter.Lift_Offset);
                NotifyPropertyChanged("Offset_Lift");
            }
        }
        public double Offset_Turntable
        {
            get
            {
                return this.offset_Turntable;
            }
            set
            {
                this.offset_Turntable = value;
                SavePositionerParameter(EnumPositionerParameter.Turntable_Offset);
                NotifyPropertyChanged("Offset_Turntable");
            }
        }
        
        public bool IsMovingStop_Slide
        {
            get
            {
                return this.isMovingStop_Slide;
            }
            set
            {
                this.isMovingStop_Slide = value;
                NotifyPropertyChanged("IsMovingStop_Slide");
            }
        }

        public bool IsMovingStop_Lift
        {
            get
            {
                return this.isMovingStop_Lift;
            }
            set
            {
                this.isMovingStop_Lift = value;
                NotifyPropertyChanged("IsMovingStop_Lift");
            }
        }

        public bool IsMovingStop_Turntable
        {
            get
            {
                return this.isMovingStop_Turntable;
            }
            set
            {
                this.isMovingStop_Turntable = value;
                NotifyPropertyChanged("IsMovingStop_Turntable");
            }
        }

        public int CurrentSpeed_Slide
        {
            get
            {
                return this.currentSpeed_Slide;
            }
            set
            {
                this.currentSpeed_Slide = value;
                NotifyPropertyChanged("CurrentSpeed_Slide");
            }
        }

        public int CurrentSpeed_Lift
        {
            get
            {
                return this.currentSpeed_Lift;
            }
            set
            {
                this.currentSpeed_Lift = value;
                NotifyPropertyChanged("CurrentSpeed_Lift");
            }
        }

        public int CurrentSpeed_Turntable
        {
            get
            {
                return this.currentSpeed_Turntable;
            }
            set
            {
                this.currentSpeed_Turntable = value;
                NotifyPropertyChanged("CurrentSpeed_Turntable");
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
                                                               new XElement("offset_Slide", "0"),
                                                               new XElement("Offset_Lift", "0"),
                                                               new XElement("offset_Turntable","0"))));                
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

                // VISA Address
                string addr = rootNode.Element("VisaAddress").Value;
                if(addr!=string.Empty)
                {
                    VisaAddress = addr;
                }
                else
                {
                    VisaAddress = "TCPIP0::192.168.127.254::4001::SOCKET";
                }
                
                // Offset_Slide
                try
                {
                    Offset_Slide = Convert.ToDouble(rootNode.Element("PositionerOffset").Element("Offset_Slide").Value);
                }
                catch
                {
                    Offset_Slide = 0;
                }

                // Offset_Lift
                try
                {
                    Offset_Lift = Convert.ToDouble(rootNode.Element("PositionerOffset").Element("Offset_Lift").Value);
                }
                catch
                {
                    Offset_Lift = 0;
                }

                // Offset_Turntable
                try
                {
                    Offset_Turntable = Convert.ToDouble(rootNode.Element("PositionerOffset").Element("Offset_Turntable").Value);
                }
                catch
                {
                    Offset_Turntable = 0;
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
                        rootNode.Element("PositionerOffset").SetElementValue("offset_Slide", this.offset_Slide.ToString("#.##"));
                        break;
                    case EnumPositionerParameter.Lift_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("Offset_Lift", this.Offset_Lift.ToString("#.##"));
                        break;
                    case EnumPositionerParameter.Turntable_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("offset_Turntable", this.offset_Turntable.ToString("#.##"));
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
