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
        Slide_Speed,
        Slide_Offset,
        Lift_Speed,
        Lift_Offset,
        Turntable_Speed,
        Turntable_Offset
    }

    // Positioner Parameters
    public class PositionerParams: INotifyPropertyChanged
    {
        // Constructor
        public PositionerParams()
        {
            ConfigXmlFolder = @"C:\Temp";             
            ConfigXML = ConfigXmlFolder + "\\" + "PositionerConfiguration.xml";

            if (!Directory.Exists(ConfigXmlFolder))
            {
                Directory.CreateDirectory(ConfigXmlFolder);
            }

            // Load Configuration XML file if it exists, otherwise create it            
            if (!File.Exists(ConfigXML))
            {
                CreateConfigXML();  // create
            }

            LoadConfigXML(); // load Configuration XML file
        }

        // ------------ Field ------------
        private string visaAddr = string.Empty;
        // Speed
        private string speed_Slide = "20";
        private string speed_Lift = "20";
        private string speed_Turntable = "20";
        
        // Offset
        private string offset_Slide     = "0";
        private string offset_Lift      = "0";
        private string offset_Turntable = "0";

        // Stopped Flag
        private bool isStopped_Slide     = true;
        private bool isStopped_Lift      = true;
        private bool isStopped_Turntable = true;

        // Current Position
        private double currPos_Slide = -99999;
        private double currPos_Lift = -99999;
        private double currPos_Turntable = -99999;

        // Target Absolute Position
        private double targetAbsPos_Slide     = 0.0;
        private double targetAbsPos_Lift      = 0.0;
        private double targetAbsPos_Turntable = 0.0;

        // Target Relative Direction: True ==> "+" / False ==> "-"
        // Slide:     "+": direction Keeping away from the door / "-": direction approching the door 
        // Lift:      "+": Up / "1": Down
        // Turntable: "+": Clockwise / "-": CounterClockwise
        private bool targetRelDir_Slide     = true;
        private bool targetRelDir_Lift      = true;
        private bool targetRelDir_Turntable = true;

        // Target Relative Position
        private double targetRelPos_Slide     = 0.0;
        private double targetRelPos_Lift      = 0.0;
        private double targetRelPos_Turntable = 0.0;

        // Locker
        static private object locker = new object();

        // ------------ Property ------------
        private string ConfigXML { set; get; }
        private string ConfigXmlFolder { set; get; }

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

        // Speed
        public string Speed_Slide
        {
            get
            {
                return this.speed_Slide;
            }
            set
            {
                this.speed_Slide = value;
                SavePositionerParameter(EnumPositionerParameter.Slide_Speed);
                NotifyPropertyChanged("Speed_Slide");
            }
        }
        public string Speed_Lift
        {
            get
            {
                return this.speed_Lift;
            }
            set
            {
                this.speed_Lift = value;
                SavePositionerParameter(EnumPositionerParameter.Lift_Speed);
                NotifyPropertyChanged("Speed_Lift");
            }
        }
        public string Speed_Turntable
        {
            get
            {
                return this.speed_Turntable;
            }
            set
            {
                this.speed_Turntable = value;
                SavePositionerParameter(EnumPositionerParameter.Turntable_Speed);
                NotifyPropertyChanged("Speed_Turntable");
            }
        }

        // OffSet
        public string Offset_Slide
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
        public string Offset_Lift
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
        public string Offset_Turntable
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
        
        // Stopped Flag
        public bool IsStopped_Slide
        {
            get
            {
                return this.isStopped_Slide;
            }
            set
            {
                this.isStopped_Slide = value;
                NotifyPropertyChanged("IsStopped_Slide");
            }
        }
        public bool IsStopped_Lift
        {
            get
            {
                return this.isStopped_Lift;
            }
            set
            {
                this.isStopped_Lift = value;
                NotifyPropertyChanged("IsStopped_Lift");
            }
        }
        public bool IsStopped_Turntable
        {
            get
            {
                return this.isStopped_Turntable;
            }
            set
            {
                this.isStopped_Turntable = value;
                NotifyPropertyChanged("IsStopped_Turntable");
            }
        }

        // Current Position
        public double CurrentPosition_Slide
        {
            get
            {
                return this.currPos_Slide;
            }
            set
            {
                this.currPos_Slide = value;
                NotifyPropertyChanged("CurrentPosition_Slide");
            }
        }
        public double CurrentPosition_Lift
        {
            get
            {
                return this.currPos_Lift;
            }
            set
            {
                this.currPos_Lift = value;
                NotifyPropertyChanged("CurrentPosition_Lift");
            }
        }
        public double CurrentPosition_Turntable
        {
            get
            {
                return this.currPos_Turntable;
            }
            set
            {
                this.currPos_Turntable = value;
                NotifyPropertyChanged("CurrentPosition_Turntable");
            }
        }

        // Target Absolute Postiont
        public double TargetAbsolutePosition_Slide
        {
            get
            {
                return this.targetAbsPos_Slide;
            }
            set
            {
                this.targetAbsPos_Slide = Math.Round(value,1);
                NotifyPropertyChanged("TargetAbsolutePosition_Slide");
            }
        }        
        public double TargetAbsolutePosition_Lift
        {
            get
            {
                return this.targetAbsPos_Lift;
            }
            set
            {
                this.targetAbsPos_Lift = Math.Round(value,1);
                NotifyPropertyChanged("TargetAbsolutePosition_Lift");
            }
        }
        public double TargetAbsolutePosition_Turntable
        {
            get
            {
                return this.targetAbsPos_Turntable;
            }
            set
            {
                this.targetAbsPos_Turntable = Math.Round(value,1);
                NotifyPropertyChanged("TargetAbsolutePosition_Turntable");
            }
        }

        // Target Relative Direction
        public bool TargetRelativeDirection_Slide
        {
            get
            {
                return this.targetRelDir_Slide;
            }
            set
            {
                this.targetRelDir_Slide = value;
                NotifyPropertyChanged("TargetRelativeDirection_Slide");
            }
        }
        public bool TargetRelativeDirection_Lift
        {
            get
            {
                return this.targetRelDir_Lift;
            }
            set
            {
                this.targetRelDir_Lift = value;
                NotifyPropertyChanged("TargetRelativeDirection_Lift");
            }
        }
        public bool TargetRelativeDirection_Turntable
        {
            get
            {
                return this.targetRelDir_Turntable;
            }
            set
            {
                this.targetRelDir_Turntable = value;
                NotifyPropertyChanged("TargetRelativeDirection_Turntable");
            }
        }

        // Target Relative Position
        public double TargetRelativePosition_Slide
        {
            get
            {
                return this.targetRelPos_Slide;
            }
            set
            {
                this.targetRelPos_Slide = Math.Round(value,1);
                NotifyPropertyChanged("TargetRelativePosition_Slide");
            }
        }
        public double TargetRelativePosition_Lift
        {
            get
            {
                return this.targetRelPos_Lift;
            }
            set
            {
                this.targetRelPos_Lift = Math.Round(value, 1);
                NotifyPropertyChanged("TargetRelativePosition_Lift");
            }
        }
        public double TargetRelativePosition_Turntable
        {
            get
            {
                return this.targetRelPos_Turntable;
            }
            set
            {
                this.targetRelPos_Turntable = Math.Round(value, 1);
                NotifyPropertyChanged("TargetRelativePosition_Turntable");
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
                                                               new XElement("Offset_Slide", "0"),
                                                               new XElement("Offset_Lift", "0"),
                                                               new XElement("Offset_Turntable","0")),
                                                            new XElement("PositionerSpeed",
                                                               new XElement("Speed_Slide", "20"),
                                                               new XElement("Speed_Lift", "20"),
                                                               new XElement("Speed_Turntable", "20"))));                
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
                lock(locker)
                {
                    XDocument configXmlDoc = XDocument.Load(ConfigXML);
                    XElement rootNode = configXmlDoc.Element("Configuration");

                    // ------------ VISA Address ------------
                    string addr = rootNode.Element("VisaAddress").Value;
                    if (addr != string.Empty)
                    {
                        VisaAddress = addr;
                    }
                    else
                    {
                        VisaAddress = "TCPIP0::192.168.127.254::4001::SOCKET";
                    }

                    // ------------ Offset ------------                
                    try
                    {
                        Offset_Slide = rootNode.Element("PositionerOffset").Element("Offset_Slide").Value;
                    }
                    catch
                    {
                        Offset_Slide = "0";
                    }

                    try
                    {
                        Offset_Lift = rootNode.Element("PositionerOffset").Element("Offset_Lift").Value;
                    }
                    catch
                    {
                        Offset_Lift = "0";
                    }

                    try
                    {
                        Offset_Turntable = rootNode.Element("PositionerOffset").Element("Offset_Turntable").Value;
                    }
                    catch
                    {
                        Offset_Turntable = "0";
                    }

                    // ------------ Speed ------------
                    try
                    {
                        speed_Slide = rootNode.Element("PositionerSpeed").Element("Speed_Slide").Value;
                    }
                    catch
                    {
                        speed_Slide = "20";
                    }

                    try
                    {
                        speed_Lift = rootNode.Element("PositionerSpeed").Element("Speed_Lift").Value;
                    }
                    catch
                    {
                        speed_Lift = "20";
                    }

                    try
                    {
                        speed_Turntable = rootNode.Element("PositionerSpeed").Element("Speed_Turntable").Value;
                    }
                    catch
                    {
                        speed_Turntable = "20";
                    }
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
                        rootNode.Element("PositionerOffset").SetElementValue("Offset_Slide", this.offset_Slide);
                        break;
                    case EnumPositionerParameter.Lift_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("Offset_Lift", this.Offset_Lift);
                        break;
                    case EnumPositionerParameter.Turntable_Offset:
                        rootNode.Element("PositionerOffset").SetElementValue("Offset_Turntable", this.offset_Turntable);
                        break;
                    case EnumPositionerParameter.Slide_Speed:
                        rootNode.Element("PositionerSpeed").SetElementValue("Speed_Slide", this.speed_Slide);
                        break;
                    case EnumPositionerParameter.Lift_Speed:
                        rootNode.Element("PositionerSpeed").SetElementValue("Speed_Lift", this.speed_Lift);
                        break;
                    case EnumPositionerParameter.Turntable_Speed:
                        rootNode.Element("PositionerSpeed").SetElementValue("Speed_Turntable", this.speed_Turntable);
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

    public class ReverseBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (bool)value;
            return !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = (bool)value;
            return !result;
        }
    }
}
