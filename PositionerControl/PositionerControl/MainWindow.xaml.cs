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
using System.Threading;
using ETSL.InstrDriver.Base;
using ETSL.InstrDriver;

namespace PositionerControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Constructor
        public MainWindow()
        {
            InitializeComponent();
            //Test();

            InitializeDmdPositioner();
        }

        // Field


        // Property
        private PositionerParams DmdPositionerParams { set; get; }
        private InstrDrvUtility DriverUtility { set; get; }
        private DmdPositionerSuite DmdPositioner { set; get; }
        
        // Method
        private void InitializeDmdPositioner()
        {
            DmdPositionerParams = (PositionerParams)this.FindResource("positionerParams");
            DriverUtility = (InstrDrvUtility)this.FindResource("instrDrvUtility");
            DmdPositioner = (DmdPositionerSuite)this.FindResource("dmdPositionerSuite");

            // Open Positioner Suite
            DmdPositioner.VisaAddress = DmdPositionerParams.VisaAddress;
            if(this.cbVisaAddrList.Items.IsEmpty)
            {
                this.cbVisaAddrList.Items.Add(DmdPositioner.VisaAddress);
            }            
            this.cbVisaAddrList.SelectedIndex = 0;
            DmdPositioner.Initialize();
            this.tblockStatus.Text = DmdPositioner.InstrID;
        }

        private void Test()
        {
            //VisaInstrDriver instrDrv = new VisaInstrDriver();
            //instrDrv.VisaAddress = "TCPIP0::192.168.127.254::4001::SOCKET";
            //instrDrv.Initialize();
            ////instrDrv.SendCommand("AXIS3:*IDN?");
            ////string resp = instrDrv.ReadCommand();
            //string resp = instrDrv.QueryCommand("AXIS3:*IDN?");
            //resp = instrDrv.QueryCommand("AXIS3:CP?");

            DmdPositionerSuite positioner = new DmdPositionerSuite();
            positioner.VisaAddress = "TCPIP0::localhost::9001::SOCKET";
            positioner.Initialize();
            positioner.Slide.Home();
            positioner.Slide.SeekPosition(20);
        }

        // Search VISA Instruments
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            tblockStatus.Text = "正在搜索连接的仪器设备...";            
            
            // Search VISA resources
            bool result = await DriverUtility.FindVisaResourcesAsync();

            if (result)
            {
                tblockStatus.Text = "设备连接列表已更新，请选择所需连接的设备！";

                this.cbVisaAddrList.Items.Clear();
                foreach (string addr in DriverUtility.VisaAddressList)
                {
                    this.cbVisaAddrList.Items.Add(addr);
                }                
            }
            else
            {
                tblockStatus.Text = "没有找到连接的设备，请检查连接！";
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(DmdPositioner!=null)
            {
                DmdPositioner.DeInitialize();
            }            
        }

        // Re-Initialize Positioner
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DmdPositioner.DeInitialize();
            Thread.Sleep(200);

            // Open Positioner Suite            
            DmdPositioner.VisaAddress = DmdPositionerParams.VisaAddress = this.cbVisaAddrList.SelectedItem.ToString();            
            DmdPositioner.Initialize();
            this.tblockStatus.Text = DmdPositioner.InstrID;
        }
    }    
}
