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
using ETSL.TcpSocket;

namespace DoorMonitor
{
    public class DoorMonitorParams: INotifyPropertyChanged
    {
        // ---------- Constructor ---------- 
        public DoorMonitorParams()
        {
            VisaAddressList = new ObservableCollection<string>();

            // Load Configuration XML file if it exists, otherwise create it            
            if (!File.Exists(this.configFilePath))
            {
                CreateConfigXML();  // create
            }
            
            LoadConfigXML(); // load Configuration XML file            
        }

        // ---------- Field ----------
        private string tileSvrName = "TILE! DoorMonitor Server";
        private UInt16 tileSvrPort = 8001;
        private string remoteIoIpAddr = "192.168.0.200";
        private UInt16 remoteIoPort = 502;
        private int visaAddrListSelIndex = -1;
        private string sgVisaAddr = string.Empty;        
        private string sgRfOffCmd = string.Empty;
        private bool initializedSG = false;
        private string sgID = string.Empty;
        private string configFilePath = @"C:\Temp\Configuration.xml";

        // ---------- Property ----------
        public string TileServerName
        {
            set
            {
                tileSvrName = value;
                NotifyPropertyChanged("TileServerName");
            }
            get
            {
                return this.tileSvrName;
            }
        }

        public UInt16 TileServerPort
        {
            set
            {
                this.tileSvrPort = value;
                NotifyPropertyChanged("TileServerPort");
            }
            get
            {
                return this.tileSvrPort;
            }
        }

        public string RemoteIoIpAddress
        {
            set
            {
                this.remoteIoIpAddr = value;
                NotifyPropertyChanged("RemoteIoIpAddress");
            }
            get
            {
                return this.remoteIoIpAddr;
            }
        }

        public UInt16 RemoteIoPort
        {
            set
            {
                this.remoteIoPort = value;
                NotifyPropertyChanged("RemoteIoPort");
            }
            get
            {
                return this.remoteIoPort;
            }
        }

        public int VisaAddrListSelIndex
        {
            set
            {
                this.visaAddrListSelIndex = value;
                NotifyPropertyChanged("VisaAddrListSelIndex");
            }
            get
            {
                return this.visaAddrListSelIndex;
            }
        }

        public string SgVisaAddress
        {
            set
            {
                this.sgVisaAddr = value;
                NotifyPropertyChanged("SgVisaAddress");
            }
            get
            {
                return this.sgVisaAddr;
            }
        }

        public string SgRfOffCommand
        {
            set
            {
                this.sgRfOffCmd = value;
                NotifyPropertyChanged("SgRfOffCommand");
            }
            get
            {
                return this.sgRfOffCmd;
            }
        }

        public bool InitializedSG
        {
            set
            {
                this.initializedSG = value;
                NotifyPropertyChanged("InitializedSG");
            }
            get
            {
                return this.initializedSG;
            }
        }

        public string SgID
        {
            set
            {
                this.sgID = value;
                NotifyPropertyChanged("SgID");
            }
            get
            {
                return this.sgID;
            }
        }

        public string ConfigFilePath
        {
            set
            {
                this.configFilePath = value;
                NotifyPropertyChanged("ConfigFilePath");
            }
            get
            {
                return this.configFilePath;
            }
        }

        public ObservableCollection<string> VisaAddressList { set; get; }        

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

        private void CreateConfigXML()
        {
            //string strCurFdr = System.IO.Directory.GetCurrentDirectory(); // Get current directory            
            
            try
            {
                XDocument configXmlDoc = new XDocument(new XElement("Configuration",
                                                           new XElement("RemoteIoAddress", "192.168.0.200"),
                                                           new XElement("RemoteIoPort", "502"),
                                                           new XElement("TileServerName", "TILE! DoorMonitor Server"),
                                                           new XElement("TileServerPort", "8001"),
                                                           new XElement("VisaAddressList", ""),
                                                           new XElement("VisaAddressListSelIndex", "-1"),                                                                                        
                                                           new XElement("SgRfOffCommand", "")));

                this.configFilePath = @"C:\Temp\Configuration.xml";
                configXmlDoc.Save(this.configFilePath);                
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Create \"C:\\Temp\\Configuration.xml\" Error!\n{0}", ex.Message);
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadConfigXML()
        {              
            try
            {
                XDocument configXmlDoc = XDocument.Load(this.configFilePath);
                XElement rootNode = configXmlDoc.Element("Configuration");
                string strRemoteIoAddr = rootNode.Element("RemoteIoAddress").Value;
                string strRemoteIoPort = rootNode.Element("RemoteIoPort").Value;
                string strTileSvrName = rootNode.Element("TileServerName").Value;
                string strTileSvrPort = rootNode.Element("TileServerPort").Value;
                string strVisaAddrList = rootNode.Element("VisaAddressList").Value;
                string strAddrListSelIndex = rootNode.Element("VisaAddressListSelIndex").Value;                
                string strSgRfOffCmd = rootNode.Element("SgRfOffCommand").Value;                

                this.remoteIoIpAddr = strRemoteIoAddr;
                this.RemoteIoPort = Convert.ToUInt16(strRemoteIoPort);
                this.tileSvrName = strTileSvrName;
                this.TileServerPort = Convert.ToUInt16(strTileSvrPort);                
                this.sgRfOffCmd = strSgRfOffCmd;
                this.visaAddrListSelIndex = Convert.ToInt16(strAddrListSelIndex);
                
                if (strVisaAddrList!=string.Empty)
                {
                    string[] list = strVisaAddrList.Split(new string[] { ";" }, StringSplitOptions.None);
                    foreach(string addr in list)
                    {
                        VisaAddressList.Add(addr);
                    }
                }

                if (this.visaAddrListSelIndex != -1)
                {
                    this.sgVisaAddr = VisaAddressList[this.visaAddrListSelIndex];
                }
            }
            catch (Exception ex)
            {
                string errMsg = string.Format("Load configuration file \"{0}\" failed!\n{1}", ConfigFilePath, ex.Message);
                MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }

    // ---------- Converter Class ----------
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

    public class TraceTextToSaveBtnEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = (string)value;
            if(text==string.Empty)
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

    public class SgInitStateToFillColorConverter : IValueConverter
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

            if(InitState)
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

    public class SgInitStateToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool InitState = (bool)value;

            if (InitState)
            {                
                return new SolidColorBrush(Colors.Blue);
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

    public class SgIdTextToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string IdText = (string)value;

            if (IdText== "Failed to read out SG ID!")
            {
                return new SolidColorBrush(Colors.Red); 
            }
            else
            {
                return new SolidColorBrush(Colors.Blue);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class SgInitStateToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool InitState = (bool)value;
           
            if (InitState)
            {
                return "SG Connected:";
            }
            else
            {
                return "Failed to connect to SG!";
            }            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException("Not implement <IValueConverter.ConverBack> function");
        }
    }

    public class VisaAddressList : ObservableCollection<string>
    {
        public VisaAddressList() : base()
        {

        }
    }
}