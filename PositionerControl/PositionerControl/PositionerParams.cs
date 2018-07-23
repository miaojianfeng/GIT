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
    public class PositionerParams: INotifyPropertyChanged
    {
        // Constructor
        public PositionerParams()
        {
            ConfigXML = System.IO.Directory.GetCurrentDirectory() + "\\PositionerConfiguration.xml"; 

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

                string offset_Slide = rootNode.Element("SlideOffset").Value;
                try
                {
                    SlideOffset = Convert.ToDouble(offset_Slide);
                }
                catch
                {
                    SlideOffset = 0;
                }

                string offset_Lift = rootNode.Element("LiftOffset").Value;
                try
                {
                    LiftOffset = Convert.ToDouble(offset_Lift);
                }
                catch
                {
                    LiftOffset = 0;
                }

                string offset_TT = rootNode.Element("TurntableOffset").Value;
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

        //private void SaveConfiguration()
    }
}
