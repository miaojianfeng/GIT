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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Ivi.Visa;
using NationalInstruments.Visa;

namespace EMPower
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum EnumConnectStep
        {
            Unknown,
            OpenSerialPort,
            SetGenericParams,
            SetSerialPortParams,
            IdnQuerying,
            Reset
        }

        enum EnumFrequencyUnit
        {
            Hz,
            kHz,
            MHz,
            GHz
        }

        private Dictionary<string, string> dictSerialPort = new Dictionary<string, string>();
        private MessageBasedSession mbSession;
        ResourceManager rmSession = null;
        ISerialSession serialSession = null;

        private string infoText = string.Empty;
        private string selComPort = string.Empty;
        static private object locker = new object();

        private EnumConnectStep ConnectStep { set; get; }
        private string InfoText
        {
            set
            {
                lock (locker)
                {
                    this.infoText = value;
                }            
            }
            get
            {
                lock (locker)
                {
                    return this.infoText;
                }
            }
        }

        private EMPowerParams Params { set; get; }
        private string SelectedComPort
        {
            set
            {
                lock(locker)
                {
                    this.selComPort = value;
                }
            }
            get
            {
                lock(locker)
                {
                    return this.selComPort;
                }
            }
        }
        private string LastSelectedComPort { set; get; }

        private int SelComPortIndex { set; get; }
        private int LastSelComPortIndex { set; get; }

        private double FrequencyNumber { set; get; }
        private EnumFrequencyUnit FrequencyUnit { set; get; }

        private string GetVisaAddrString(string serialPortName)
        {
            return this.dictSerialPort[serialPortName];
        }

        public MainWindow()
        {
            InitializeComponent();

            Params = (EMPowerParams)this.FindResource("Params");
            InitUiElements();
            FindSerialPorts();
        }

        private void FindSerialPorts()
        {
            // This example uses an instance of the NationalInstruments.Visa.ResourceManager class to find resources on the system.
            // Alternatively, static methods provided by the Ivi.Visa.ResourceManager class may be used when an application
            // requires additional VISA .NET implementations.
            using (var rm = new ResourceManager())
            {
                try
                {
                    IEnumerable<string> resources = rm.Find("ASRL?*INSTR");
                    foreach (string s in resources)
                    {
                        ParseResult parseResult = rm.Parse(s);                        
                        if (parseResult.AliasIfExists.ToUpper().Contains("COM"))
                        {
                            if(!this.dictSerialPort.ContainsKey(parseResult.AliasIfExists))
                            {
                                this.dictSerialPort.Add(parseResult.AliasIfExists, s);
                                this.cboxSerialPort.Items.Add(parseResult.AliasIfExists);
                            }                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errMsg = "没有检测到串口！\n\n" + "详细原因：\n" + ex.Message;
                    Params.ErrorMessage = errMsg;
                    MessageBox.Show(this, errMsg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool ConnectEMPower()
        {
            bool retValue = false;
            string resp = string.Empty;
            
            string comPort = SelectedComPort;
            string visaName = GetVisaAddrString(comPort);

            try
            {
                //Open session
                ConnectStep = EnumConnectStep.OpenSerialPort;
                rmSession = new ResourceManager();
                mbSession = (MessageBasedSession)rmSession.Open(visaName);                
                ConnectStep = EnumConnectStep.SetGenericParams;
                mbSession.TerminationCharacterEnabled = true;
                byte termChar = 0b1010;  // 0xA
                mbSession.TerminationCharacter = termChar;
                mbSession.TimeoutMilliseconds = 5000;

                // Serial params settings
                ConnectStep = EnumConnectStep.SetSerialPortParams;
                serialSession = (ISerialSession)mbSession;
                serialSession.BaudRate = 115200;
                serialSession.Parity = SerialParity.None;
                serialSession.DataBits = 8;
                serialSession.StopBits = SerialStopBitsMode.One;
                serialSession.FlowControl = SerialFlowControlModes.None;

                // *IDN? querying 
                ConnectStep = EnumConnectStep.IdnQuerying;
                serialSession.RawIO.Write("*IDN?\n");
                resp = serialSession.RawIO.ReadString().TrimEnd("\r\n".ToCharArray());
                if (resp.ToUpper().Contains("ETS-LINDGREN"))
                {
                    InfoText = resp;
                    retValue = true;
                }
                else
                {
                    InfoText = "EMPower ID 信息错误！";
                    return false;
                }

                // Reset
                ConnectStep = EnumConnectStep.Reset;
                serialSession.RawIO.Write("RESET\n");
                System.Threading.Thread.Sleep(200);
                resp = serialSession.RawIO.ReadString().TrimEnd("\r\n".ToCharArray());
                if(resp.ToUpper()=="OK")
                {
                    retValue = true;
                }
                else
                {
                    InfoText = "重置设备失败";
                    return false;
                }

                // 选择滤波器：自动
                serialSession.RawIO.Write("FILTER AUTO\n");

                return retValue;
            }
            catch
            {                
                switch (ConnectStep)
                {
                    case EnumConnectStep.OpenSerialPort:
                        InfoText = string.Format("打开串口{0}失败\n请检查串口是否已被占用或者串口号指定错误", SelectedComPort);
                        break;
                    case EnumConnectStep.SetGenericParams:
                        InfoText = "设置VISA通信端口参数失败";
                        break;
                    case EnumConnectStep.SetSerialPortParams:
                        InfoText = "设置串口参数失败";
                        break;
                    case EnumConnectStep.IdnQuerying:
                        InfoText = "查询EMPower ID信息失败";
                        break;
                    case EnumConnectStep.Reset:
                        InfoText = "重置设备失败";
                        break;
                    default:
                        InfoText = "未知错误";
                        break;
                }

                return false;                
            }
        }        

        private async Task<bool> ConnectEMPowerAsync()
        {
            Task<bool> task = new Task<bool>(ConnectEMPower);
            task.Start();
            await task;
            return task.Result;
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {        
            if(LastSelectedComPort!=SelectedComPort)
            {
                DisconnectEMPower();

                Params.IsConnected = false;
                Params.FirmwareVersion = string.Empty;
                Params.ConnectStatusMessage = "连接中...";
                this.tboxFreqNum.Text = string.Empty;
                LastSelectedComPort = SelectedComPort;
                LastSelComPortIndex = SelComPortIndex;

                bool result = await ConnectEMPowerAsync();
                if (result)
                {
                    Params.IsConnected = true;
                    Params.FirmwareVersion = InfoText;
                    Params.ConnectStatusMessage = "已连接";
                    this.cboxFilter.SelectedIndex = 1;
                }
                else
                {
                    Params.IsConnected = false;
                    Params.FirmwareVersion = string.Empty;
                    Params.ConnectStatusMessage = "未连接";
                    ResetUIElements();
                    string errMsg = "连接EMPower失败！\n\n" + "详细原因：\n" + InfoText;
                    MessageBox.Show(this, errMsg, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        private void DisconnectEMPower()
        {
            if (mbSession != null)
            {
                mbSession.Dispose();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DisconnectEMPower();
        }

        private void CboxSerialPort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(Params!=null)
            {
                if (this.cboxSerialPort.SelectedIndex!=0)
                {
                    Params.IsComPortListExists = true;
                    SelectedComPort = this.cboxSerialPort.SelectedItem.ToString();
                    SelComPortIndex = this.cboxSerialPort.SelectedIndex;
                }
                else if(this.cboxSerialPort.SelectedIndex==0)
                {
                    this.cboxSerialPort.SelectedIndex = LastSelComPortIndex;
                }
                else
                {
                    Params.IsComPortListExists = false;
                    SelectedComPort = string.Empty;
                }
            }            
        }

        //using System.Text.RegularExpressions
        private void tboxFreqNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void ExpdrSettings_Collapsed(object sender, RoutedEventArgs e)
        {
            this.Height = 185;
        }

        private void ExpdrSettings_Expanded(object sender, RoutedEventArgs e)
        {
            this.Height = 250;
        }

        private void BtnSearchComPort_Click(object sender, RoutedEventArgs e)
        {
            DisconnectEMPower();
            ResetUIElements();
            FindSerialPorts();
        }

        private void ResetUIElements()
        {
            ConnectStep = EnumConnectStep.Unknown;
            Params.IsConnected = false;
            Params.IsComPortListExists = false;
            Params.ConnectStatusMessage = "未连接";
            Params.ErrorMessage = string.Empty;
            Params.FirmwareVersion = string.Empty;
            SelectedComPort = "请选择";
            LastSelectedComPort = "请选择";
            SelComPortIndex = 0;
            LastSelComPortIndex = 0;
            this.cboxSerialPort.SelectedIndex = 0;
        }

        private void InitUiElements()
        {
            ConnectStep = EnumConnectStep.Unknown;
            LastSelectedComPort = SelectedComPort = string.Empty;
            LastSelComPortIndex = SelComPortIndex = 0;
        }

        private string ReadEMPower()
        {            
            string resp = string.Empty;
            serialSession.RawIO.Write("POWER?\n");
            serialSession.Flush(IOBuffers.Read, true);
            resp = serialSession.RawIO.ReadString().TrimEnd("\r\n".ToCharArray());
            return resp;
        }

        private void btnSingle_Click(object sender, RoutedEventArgs e)
        {
            string resp = ReadEMPower();
            this.tboxPower.Text = resp;
        }
    }
}
