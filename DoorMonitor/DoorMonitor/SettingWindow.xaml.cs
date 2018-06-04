using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Linq;
using ETSL.InstrDriver.Base;
using ETSL.Utilities;
using ETSL.TcpSocket;

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        public SettingWindow(DoorMonitorParams monitorParams, InstrumentManager instrMgr, VisaInstrDriver instrDrv)
        {
            InitFinished = false;
            InitializeComponent();

            MonitorParams = monitorParams;
            InstrMgr = instrMgr;
            InstrDrv = instrDrv;
            InitFinished = true;             
        }

        // Property
        private DoorMonitorParams MonitorParams { set; get; }
        private InstrumentManager InstrMgr { set; get; }
        private VisaInstrDriver InstrDrv { set; get; }
        public Action<string> UpdateTrace { set; get; }
        public Action<bool> SetParamsChangedFlag { set; get; }

        private bool InitFinished { set; get; }

        private bool HasVisaAddrListChanged
        {        
            get
            {
                bool flag = false;

                try
                {
                    StringBuilder sb = new StringBuilder();
                    int count = MonitorParams.VisaAddressList.Count;
                    string addrListActual = string.Empty;
                    if (count != 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (i != count - 1)
                            {
                                sb.Append(MonitorParams.VisaAddressList[i] + ";");
                            }
                            else
                            {
                                sb.Append(MonitorParams.VisaAddressList[i]);
                            }
                        }
                        addrListActual = sb.ToString();
                    }
                    else
                    {
                        addrListActual = string.Empty;
                    }

                    XDocument configXmlDoc = XDocument.Load(MonitorParams.ConfigFilePath);
                    XElement rootNode = configXmlDoc.Element("Configuration");
                    string addrListXML = rootNode.Element("VisaAddressList").Value;
                    if (addrListXML == addrListActual)
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Save <C:\\Temp\\Configuration.xml> Error!");
                    flag = false;
                }

                return flag;
            }                        
        }       
        
        private async void btnSearchSG_Click(object sender, RoutedEventArgs e)
        {
            tbAddr.Text = "Search SG is in progress...";
            // Search VISA resources
            bool result = await InstrMgr.FindVisaResourcesAsync();           

            if (result)
            {
                tbAddr.Text = "VISA Instruments founded!";
                
                MonitorParams.VisaAddressList.Clear();
                foreach (string addr in InstrMgr.VisaAddressList)
                {
                    MonitorParams.VisaAddressList.Add(addr);
                }
                
                if(HasVisaAddrListChanged)
                {
                    //this.cbVisaAddrList.SelectedIndex = 0;
                }                

                tbAddr.Text = "Select the SG address from the following list";
            }

        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (SetParamsChangedFlag != null) SetParamsChangedFlag(false);            
            Close();
        }

        private void SetParameters_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (InitFinished)
            {
                UInt16 remoteIoPort = Convert.ToUInt16(this.tbRemoteIoPort.Text);
                UInt16 tileSvrPort = Convert.ToUInt16(this.tbTileSvrPort.Text);

                if (this.tbRemoteIoIpAddr.Text != MonitorParams.RemoteIoIpAddress ||
                     remoteIoPort != MonitorParams.RemoteIoPort ||
                     this.tbTileSvrName.Text != MonitorParams.TileServerName ||
                     tileSvrPort != MonitorParams.TileServerPort ||
                     this.tbRfOffCmd.Text != MonitorParams.SgRfOffCommand ||
                     //HasVisaAddrListChanged ||
                     this.cbVisaAddrList.SelectedIndex != MonitorParams.VisaAddrListSelIndex)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }
            }                
                    
            e.Handled = true;
        }

        private void SetParameters_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                XDocument configXmlDoc = XDocument.Load(MonitorParams.ConfigFilePath);
                XElement rootNode = configXmlDoc.Element("Configuration");

                if (this.tbRemoteIoIpAddr.Text != MonitorParams.RemoteIoIpAddress)
                {
                    MonitorParams.RemoteIoIpAddress = this.tbRemoteIoIpAddr.Text;
                    rootNode.SetElementValue("RemoteIoAddress", this.tbRemoteIoIpAddr.Text);
                }

                UInt16 remoteIoPort = Convert.ToUInt16(this.tbRemoteIoPort.Text);
                if (remoteIoPort != MonitorParams.RemoteIoPort)
                {
                    MonitorParams.RemoteIoPort = remoteIoPort;
                    rootNode.SetElementValue("RemoteIoPort", this.tbRemoteIoPort.Text);
                }

                if (this.tbTileSvrName.Text != MonitorParams.TileServerName)
                {
                    MonitorParams.TileServerName = this.tbTileSvrName.Text;
                    rootNode.SetElementValue("TileServerName", this.tbTileSvrName.Text);
                }

                UInt16 tileSvrPort = Convert.ToUInt16(this.tbTileSvrPort.Text);
                if (tileSvrPort != MonitorParams.TileServerPort)
                {
                    MonitorParams.TileServerPort = tileSvrPort;
                    rootNode.SetElementValue("TileServerPort", this.tbTileSvrPort.Text);
                }

                if (this.tbRfOffCmd.Text != MonitorParams.SgRfOffCommand)
                {
                    MonitorParams.SgRfOffCommand = this.tbRfOffCmd.Text;
                    rootNode.SetElementValue("SgRfOffCommand", this.tbRfOffCmd.Text);
                }

                string visaList = GetVisaListString();
                if (rootNode.Element("VisaAddressList").Value != visaList || this.cbVisaAddrList.SelectedIndex != MonitorParams.VisaAddrListSelIndex)
                {
                    if (this.cbVisaAddrList.SelectedIndex != MonitorParams.VisaAddrListSelIndex)
                    {
                        MonitorParams.VisaAddrListSelIndex = this.cbVisaAddrList.SelectedIndex;
                        rootNode.SetElementValue("VisaAddressListSelIndex", this.cbVisaAddrList.SelectedIndex.ToString());
                    }

                    if (rootNode.Element("VisaAddressList").Value != visaList)
                    {
                        MonitorParams.SgVisaAddress = MonitorParams.VisaAddressList[this.cbVisaAddrList.SelectedIndex];
                        rootNode.SetElementValue("VisaAddressList", visaList);
                    }
                }

                configXmlDoc.Save(MonitorParams.ConfigFilePath);

                if (SetParamsChangedFlag != null) SetParamsChangedFlag(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save <C:\\Temp\\Configuration.xml> Error!");
            }

            e.Handled = true;
            Close();            
        }

        private void TestCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(this.tbRfOffCmd.Text!=string.Empty)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;

            }
            e.Handled = true;
        }

        private void TestCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.tbRfOffCmd.Text != MonitorParams.SgRfOffCommand)
            {
                //MonitorParams.SgRfOffCommand = this.tbRfOffCmd.Text;
            }

            // Send SCPI Commands

            // Update Trace to MainWindow
            string msg = string.Format("Send SCPI Command <{0}> to <{1}>.", this.tbRfOffCmd.Text, MonitorParams.SgVisaAddress);
            AppendTrace(EnumTraceType.Information, msg);

            e.Handled = true;
        }

        private void AppendTrace(EnumTraceType traceType, string message)
        {
            // Add time stamp in the beginning of the trace record
            string timeStamp = "[ " + Auxiliaries.TimeStampGenerate() + " ]";

            // Trace type
            string typeStr = string.Empty;
            switch (traceType)
            {
                case EnumTraceType.Information:
                    typeStr = "[ INF ]";
                    break;
                case EnumTraceType.Error:
                    typeStr = "[ ERR ]";
                    break;
                case EnumTraceType.Exception:
                    typeStr = "[ EXC ]";
                    break;
                case EnumTraceType.Message:
                    typeStr = "[ MSG ]";
                    break;
            }

            // Trace body
            if (!message.EndsWith("\n"))
            {
                message += "\n";
            }

            string traceText = timeStamp + " " + typeStr + "   " + message;
            
            if(UpdateTrace!=null) UpdateTrace(traceText);
        }

        private string GetVisaListString()
        {
            StringBuilder sbVisaList = new StringBuilder();
            for (int i = 0; i < this.cbVisaAddrList.Items.Count; i++)
            {
                if (i != this.cbVisaAddrList.Items.Count - 1)
                {
                    sbVisaList.Append(this.cbVisaAddrList.Items[i] + ";");
                }
                else
                {
                    sbVisaList.Append(this.cbVisaAddrList.Items[i]);
                }
            }

            return sbVisaList.ToString();
        }        
    }    
}
