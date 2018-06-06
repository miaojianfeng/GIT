﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Xml.Linq;
using ETSL.TcpSocket;
using ETSL.Utilities;
using ETSL.InstrDriver.Base;

namespace DoorMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Field                
        private TraceWindow traceWnd;
        private SettingWindow settingWnd;
        private bool isTraceWndOpened = false;
        private WindowState lastWindowState;

        private DoorMonitorParams MonitorParams { set; get; }
        private TcpSocketServer TcpServer { set; get; }
        private ModbusTcpSocketClient ModbusTcpClient { set; get; }
        private VisaInstrDriver SgDriver { set; get; }
        public InstrumentManager InstrMgr { private set; get; }
        public VisaInstrDriver InstrDrv { private set; get; }
        public bool ParamsChanged { set; get; }
        public Action<string> UpdateTrace { get; set; }

        /// <summary>
        /// Constructor 
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            // Instance Trace Window 
            this.traceWnd = new TraceWindow();
            this.traceWnd.CloseTraceWnd = FireChkboxUnCheckedEvent_ShowTraceWnd;
            HideTraceWnd();

            // Flag used to decide whether to destroy window or just minimize the window
            DestroyMainWnd = false;            
            this.Show();

            UpdateTrace = this.traceWnd.UpdateTrace;       

            InstrMgr = (InstrumentManager)this.FindResource("InstrMgr");
            InstrDrv = (VisaInstrDriver)this.FindResource("VisaInstrDrv");
            MonitorParams = (DoorMonitorParams)this.FindResource("doorMonitorParams");
            TcpServer = (TcpSocketServer)this.FindResource("tcpServer");
            ModbusTcpClient = (ModbusTcpSocketClient)this.FindResource("modbusTcpClient");
            ParamsChanged = false;            
            StartDoorMonitor();
        }

        // Property
        /// <summary>
        /// IsTraceWndOpened
        /// </summary>
        private bool IsTraceWndOpened
        {
            set
            {
                this.isTraceWndOpened = value;
                NotifyPropertyChanged("IsTraceWndOpened");
            }
            get
            {
                return this.isTraceWndOpened;
            }
        }

        private bool DestroyMainWnd { get; set; }

        private double MainWndLeftPos { set; get; }
        private double MainWndTopPos { set; get; }

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

        public void ShowTraceWnd()
        {
            double left, top;
            double widthWorkArea = SystemParameters.WorkArea.Width;  // Get the work area width
            double heightWorkArea = SystemParameters.WorkArea.Height; // Get the work area Height
            left = widthWorkArea - this.traceWnd.Width;
            top = heightWorkArea - this.traceWnd.Height;
            this.traceWnd.Left = left;
            this.traceWnd.Top = top;
            this.traceWnd.Show();
            IsTraceWndOpened = true;            
        }

        public void HideTraceWnd()
        {
            this.traceWnd.Hide();
            IsTraceWndOpened = false;           
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

            if (UpdateTrace != null) UpdateTrace(traceText);
        }

        private string ProcessCommand(string command)
        {
            string respMsg = "No Response";
            switch (command.Trim())
            {
                case "0-Door Check":
                    if(this.ModbusTcpClient!=null)
                    {
                        if (this.ModbusTcpClient.IsDoor1Open == EnumDoorStatus.Open || this.ModbusTcpClient.IsDoor2Open == EnumDoorStatus.Open)
                        {
                            respMsg = "Fail";
                        }
                        else
                        {
                            respMsg = "Pass";
                        }
                    }
                    else
                    {
                        respMsg = "Pass";
                    }
                                        
                    break;
                default:
                    break;
            }

            return respMsg;
        }        

        public void FireChkboxUnCheckedEvent_ShowTraceWnd()
        {
            this.chkboxShowTrace.IsChecked = false;           
        }
        #endregion

        // EventHandler
        #region EventHandler    
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.lastWindowState = WindowState;
            this.Hide();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            else
            {
                this.lastWindowState = this.WindowState;
            }
        }
        
        public void ShowBalloonTip()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (this.WindowState == WindowState.Minimized)
                {
                    StringBuilder msg = new StringBuilder();
                    if (ModbusTcpClient.IsDoor1Open == EnumDoorStatus.Open)
                    {
                        msg.Append("Door1");
                    }


                    if (ModbusTcpClient.IsDoor2Open == EnumDoorStatus.Open)
                    {
                        msg.Append(" Door2");
                    }

                    this.notifyIcon.BalloonTipText = string.Format("{0} Opened!\nSignal generator is shut down.", msg.ToString().Trim());
                    this.notifyIcon.BalloonTipTitle = "Alert";
                    this.notifyIcon.ShowBalloonTip(10);
                }
            });            
        }

        public void PopupMainWindow()
        {
            this.Dispatcher.Invoke(() => 
            {
                //double left, top;
                //GetCenterScreenPosition(out left, out top);
                //this.Left = left;
                //this.Top = top;
                this.Show();
                this.WindowState = this.lastWindowState;
            });            
        }

        private void OnNotifyIconDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.Show();
                this.WindowState = this.lastWindowState;
            }
        }

        // NotifyIcon Context Menu Item <Open>
        private void OnOpenClick(object sender, RoutedEventArgs e)
        {
            this.Show();
            this.WindowState = this.lastWindowState;
        }

        // NotifyIcon Context Menu Item <Exit>
        private void OnExitClick(object sender, RoutedEventArgs e)
        {
            DestroyMainWnd = true;
            this.Close();

            // Stop Monitor
            StopDoorMonitor();

            // Close Settings Window if it is open
            if(this.settingWnd!=null)
            {
                this.settingWnd.Close();
            }            
        }

        private void ConnectToSG(string visaAddr)
        {
            if (MonitorParams.ControlSG == false)
            {
                return;
            }

            if(MonitorParams.SgVisaAddress==string.Empty)
            {
                MessageBox.Show("SG VISA address is empty!\nPlease set the correct SG address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
             
            if(SgDriver==null)
            {
                SgDriver = new VisaInstrDriver(MonitorParams.SgVisaAddress);
                AppendTrace(EnumTraceType.Information, string.Format("Connect to SG \"{0}\"...", MonitorParams.SgVisaAddress));
                SgDriver.Initialize();

                MonitorParams.InitializedSG = SgDriver.HasInitialized;
                if (MonitorParams.InitializedSG)
                {
                    AppendTrace(EnumTraceType.Information, string.Format("Connect to SG \"{0}\" successfully!", MonitorParams.SgVisaAddress));
                    AppendTrace(EnumTraceType.Information, string.Format("Send SCPI command <*IDN?> to SG \"{0}\"...", MonitorParams.SgVisaAddress));
                    SgDriver.SendCommand("*IDN?");
                    System.Threading.Thread.Sleep(300);
                    AppendTrace(EnumTraceType.Information, string.Format("Read response of SCPI command <*IDN?> from SG \"{0}\"...", MonitorParams.SgVisaAddress));
                    MonitorParams.SgID = SgDriver.ReadCommand();
                    if (MonitorParams.SgID == string.Empty)
                    {
                        MonitorParams.SgID = "Failed to read out SG ID!";
                        string errMsg = string.Format("Failed to read out the ID Information from SG<{0}>", MonitorParams.SgVisaAddress);
                        MessageBox.Show(errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    AppendTrace(EnumTraceType.Information, string.Format("SG <{0}> ID readout <{1}>", MonitorParams.SgVisaAddress, MonitorParams.SgID));
                }
                else
                {
                    AppendTrace(EnumTraceType.Information, string.Format("Failed to connect to SG <{0}>!", MonitorParams.SgVisaAddress));
                    MonitorParams.SgID = string.Empty;
                    string errMsg = string.Format("Failed to connect to SG <{0}>!\nPlease set the correct SG address or check the network connection.", MonitorParams.SgVisaAddress);
                    MessageBox.Show(this, errMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        private void DisconnectSG()
        {
            if (MonitorParams.ControlSG == false)
            {
                return;
            }

            if (SgDriver!=null)
            {
                SgDriver.DeInitialize();
                SgDriver = null;
            }
        }

        private void ShutDownRF()
        {
            if(MonitorParams.ControlSG && MonitorParams.InitializedSG)
            {
                AppendTrace(EnumTraceType.Information, string.Format("Shut down RF using SCPI command <{0}> for SG \"{1}\"", MonitorParams.SgRfOffCommand, MonitorParams.SgVisaAddress));
                SgDriver.SendCommand(MonitorParams.SgRfOffCommand);
            }
        }

        private void StartDoorMonitor()
        { 
            ConnectToSG(MonitorParams.SgVisaAddress);            
            StartRemoteIO(); 
            StartTileServer();
        }

        private void StopDoorMonitor()
        {
            DisconnectSG();
            StopRemoteIO();
            StopTileServer();
        }

        private void ResetDoorMonitor()
        {            
            StopDoorMonitor();
            StartDoorMonitor();                       
        }

        private void StartRemoteIO()
        {
            // Modbus_TCP with ZL6042
            ModbusTcpClient.IPAddress = MonitorParams.RemoteIoIpAddress; // "192.168.0.200";
            ModbusTcpClient.Port = MonitorParams.RemoteIoPort;           // 502;
            ModbusTcpClient.UpdateTrace = this.traceWnd.UpdateTrace;
            ModbusTcpClient.ShowAlertMessage = this.ShowBalloonTip;
            ModbusTcpClient.ShowMainWindow = this.PopupMainWindow;
            ModbusTcpClient.SwitchOffRF = this.ShutDownRF;
            ModbusTcpClient.Start();
        }

        private void StopRemoteIO()
        {
            if (ModbusTcpClient != null) ModbusTcpClient.Stop();                       
        }

        private void ResetRemoteIO()
        {
            StopRemoteIO();
            System.Threading.Thread.Sleep(200);
            StartRemoteIO();
        }

        private async void StartTileServer()
        {
            TcpServer.ServerName = MonitorParams.TileServerName; //"Server";
            TcpServer.ServerPort = MonitorParams.TileServerPort; //8001;
            TcpServer.EnableTrace = true;
            TcpServer.QueryTimeout_ms = 200;
            TcpServer.ProcessMessage = ProcessCommand;
            TcpServer.UpdateTrace = this.traceWnd.UpdateTrace;
            await TcpServer.Start();
        }

        private void StopTileServer()
        {
            if (TcpServer != null) TcpServer.Stop();
        }

        private void ResetTileSever()
        {
            StopTileServer();
            System.Threading.Thread.Sleep(200);
            StartTileServer();
        }

        private void chkboxShowTrace_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsTraceWndOpened)
            {
                ShowTraceWnd();
            }
        }

        private void chkboxShowTrace_Unchecked(object sender, RoutedEventArgs e)
        {
            if (IsTraceWndOpened)
            {
                HideTraceWnd();
            }
        }

        private void SaveConfigXmlControlSG()
        {
            XDocument configXmlDoc = XDocument.Load(MonitorParams.ConfigFilePath);
            XElement rootNode = configXmlDoc.Element("Configuration");
            rootNode.SetElementValue("ControlSG", MonitorParams.ControlSG? "True":"False");
            configXmlDoc.Save(MonitorParams.ConfigFilePath);
        }

        private void chkboxControlSG_Checked(object sender, RoutedEventArgs e)
        {
            if (MonitorParams != null)
            {
                ConnectToSG(MonitorParams.SgVisaAddress);

                if (SgDriver != null && SgDriver.HasInitialized)
                {
                    MonitorParams.ControlSG = true;
                }
                else
                {
                    MonitorParams.ControlSG = false;
                }

                SaveConfigXmlControlSG();
                ResetRemoteIO();
            }
        }

        private void chkboxControlSG_Unchecked(object sender, RoutedEventArgs e)
        {
            if (MonitorParams != null)
            {
                DisconnectSG();

                SaveConfigXmlControlSG();
            }
        }
        #endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(DestroyMainWnd)
            {
                if (this.traceWnd != null)
                {
                    this.traceWnd.DestroyWndFlag = true;
                    this.traceWnd.Close();
                }
            }
            else
            {
                e.Cancel = true;  // Bypass window destroy procedure but just minimize the window
                WindowState = WindowState.Minimized;
            }
        }

        private void chkboxMonitorDoor_Checked(object sender, RoutedEventArgs e)
        {
            if (ModbusTcpClient != null)
            { 
                ResetRemoteIO();
            }
        }        

        private void chkboxMonitorDoor1_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ModbusTcpClient != null)
            {                
                ModbusTcpClient.IsDoor1Open = EnumDoorStatus.Ignore;
            }
        }

        private void chkboxMonitorDoor2_Unchecked(object sender, RoutedEventArgs e)
        {
            if (ModbusTcpClient != null)
            {
                ModbusTcpClient.IsDoor2Open = EnumDoorStatus.Ignore;                
            }
        }

        private void CheckParamsChanged(bool changedFlag)
        {
            if (changedFlag) ParamsChanged = true;
            else ParamsChanged = false; 
        }

        private void btnMoreSettings_Click(object sender, RoutedEventArgs e)
        {
            this.settingWnd = new SettingWindow(MonitorParams, InstrMgr, InstrDrv);

            // Set Bindings
            // <1> RemoteIoIpAddress
            Binding binding1 = new Binding("RemoteIoIpAddress") { Source = MonitorParams };
            binding1.Mode = BindingMode.TwoWay;
            binding1.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.tbRemoteIoIpAddr.SetBinding(TextBox.TextProperty, binding1);

            // <2> RemoteIoPort
            Binding binding2 = new Binding("RemoteIoPort") { Source = MonitorParams };
            binding2.Mode = BindingMode.TwoWay;
            binding2.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.tbRemoteIoPort.SetBinding(TextBox.TextProperty, binding2);

            // <3> TileServerName
            Binding binding3= new Binding("TileServerName") { Source = MonitorParams };
            binding3.Mode = BindingMode.TwoWay;
            binding3.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.tbTileSvrName.SetBinding(TextBox.TextProperty, binding3);

            // <4> TileServerPort
            Binding binding4 = new Binding("TileServerPort") { Source = MonitorParams };
            binding4.Mode = BindingMode.TwoWay;
            binding4.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.tbTileSvrPort.SetBinding(TextBox.TextProperty, binding4);

            // <5> SgRfOffCommand
            Binding binding5 = new Binding("SgRfOffCommand") { Source = MonitorParams };
            binding5.Mode = BindingMode.TwoWay;
            binding5.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.tbRfOffCmd.SetBinding(TextBox.TextProperty, binding5);

            // <6> ConfigFilePath
            Binding binding6 = new Binding("ConfigFilePath") { Source = MonitorParams };
            binding6.Mode = BindingMode.OneWay;
            this.settingWnd.tbConfigFile.SetBinding(TextBlock.TextProperty, binding6);

            // <7> VisaAddressList
            this.settingWnd.cbVisaAddrList.ItemsSource = MonitorParams.VisaAddressList;

            // <8> VisaAddrListSelIndex
            Binding binding7 = new Binding("VisaAddrListSelIndex") { Source = MonitorParams };
            binding7.Mode = BindingMode.TwoWay;
            binding7.UpdateSourceTrigger = UpdateSourceTrigger.Explicit;
            this.settingWnd.cbVisaAddrList.SetBinding(ComboBox.SelectedIndexProperty, binding7);

            this.settingWnd.UpdateTrace = this.traceWnd.UpdateTrace;
            this.settingWnd.SetParamsChangedFlag = CheckParamsChanged;

            settingWnd.ShowDialog();            
            this.settingWnd.UpdateTrace = null;
            this.settingWnd.SetParamsChangedFlag = null;
            this.settingWnd = null;
            
            // Apply new settings
            if(ParamsChanged)
            {
                ResetDoorMonitor();
                ParamsChanged = false;
            }                          
        }
    }
}
