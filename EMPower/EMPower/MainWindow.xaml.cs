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
        // ---------- Type Definition ---------- 
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

        // ---------- Field ----------
        static private object locker = new object();
        private string comPortName = string.Empty;
        private Dictionary<string, string> dictSerialPort = new Dictionary<string, string>();
        private string infoText = string.Empty;        
        private MessageBasedSession mbSession;
        ResourceManager rmSession = null;
        ISerialSession serialSession = null;

        // ---------- Property ----------
        private EMPowerParams Params { set; get; }
        private string ComPortName
        {
            set
            {
                lock (locker)
                {
                    this.comPortName = value;
                }
            }
            get
            {
                lock (locker)
                {
                    return this.comPortName;
                }
            }
        }
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
        private double FrequencyNumber { set; get; }
        private EnumFrequencyUnit FrequencyUnit { set; get; }
        private double Frequency_kHz { set; get; }

        // ---------- Constructor ---------- 
        public MainWindow()
        {
            InitializeComponent();

            Params = (EMPowerParams)this.FindResource("Params");
            InitUiElements();
            FindComPorts();
        }

        // ---------- Method ---------- 
        private string GetVisaAddrString(string serialPortName)
        {
            return this.dictSerialPort[serialPortName];
        }

        private void FindComPorts()
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
            string visaName = string.Empty;

            try
            {
                //Open session                
                ConnectStep = EnumConnectStep.OpenSerialPort;
                visaName = GetVisaAddrString(ComPortName);
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
                    InfoText = string.Format("查询EMPower({0})信息失败", ComPortName); 
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
                    InfoText = string.Format("重置EMPower{0}失败", ComPortName); ;
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
                        InfoText = string.Format("打开串口{0}失败\n请检查串口是否已被占用或者串口号指定错误", ComPortName);
                        break;
                    case EnumConnectStep.SetGenericParams:
                        InfoText = string.Format("设置VISA通信端口{0}参数失败", ComPortName);
                        break;
                    case EnumConnectStep.SetSerialPortParams:
                        InfoText = string.Format("设置串口{0}参数失败", ComPortName);
                        break;
                    case EnumConnectStep.IdnQuerying:
                        InfoText = string.Format("查询EMPower({0})信息失败", ComPortName);
                        break;
                    case EnumConnectStep.Reset:
                        InfoText = string.Format("重置EMPower{0}失败", ComPortName);
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

        private async void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if(Params.IsConnected)  // Disconnect
            {
                DisconnectEMPower();
                Params.ComPortIndex = this.cboxSerialPort.SelectedIndex;
                Params.IsConnected = false;
                Params.FirmwareVersion = string.Empty;
                Params.ConnectStatusMessage = "未连接";
                Params.ErrorMessage = string.Empty;
                Params.PowerResult = string.Empty;
                Params.FilterIndex = 0;
                this.cboxSerialPort.IsEnabled = true;
                this.btnConnect.Content = "连接";
            }
            else  // Connect
            {
                ComPortName = this.cboxSerialPort.SelectedItem.ToString();
                Params.ComPortIndex = this.cboxSerialPort.SelectedIndex;
                Params.IsConnected = false;
                Params.FirmwareVersion = string.Empty;
                Params.ConnectStatusMessage = "连接中...";
                Params.ErrorMessage = string.Empty;
                Params.PowerResult = string.Empty;
                Params.FilterIndex = 0;                

                bool result = await ConnectEMPowerAsync();
                if (result)
                {
                    Params.IsConnected = true;
                    Params.FirmwareVersion = InfoText;
                    Params.ConnectStatusMessage = "已连接";
                    Params.FilterIndex = 1;
                    this.cboxSerialPort.IsEnabled = false;
                    this.btnConnect.Content = "断开";
                }
                else 
                {
                    Params.IsConnected = false;
                    Params.FirmwareVersion = string.Empty;
                    Params.ConnectStatusMessage = "未连接";
                    Params.FilterIndex = 0;
                    this.cboxSerialPort.IsEnabled = true;
                    this.btnConnect.Content = "连接";

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

        //using System.Text.RegularExpressions
        private void tboxFreqNum_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9.-]+");
            e.Handled = re.IsMatch(e.Text);
        }

        private void ExpdrSettings_Collapsed(object sender, RoutedEventArgs e)
        {
            this.Height = 180;
        }

        private void ExpdrSettings_Expanded(object sender, RoutedEventArgs e)
        {
            this.Height = 250;
        }

        private void BtnSearchComPort_Click(object sender, RoutedEventArgs e)
        {
            this.dictSerialPort.Clear();
            this.cboxSerialPort.SelectedIndex = 0;
            int count = this.cboxSerialPort.Items.Count;
            for (int i = count-1; i > 0; i--)
            {                
                this.cboxSerialPort.Items.RemoveAt(i);       
            }
            FindComPorts();
        }

        private void InitUiElements()
        {
            ConnectStep = EnumConnectStep.Unknown;            
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
