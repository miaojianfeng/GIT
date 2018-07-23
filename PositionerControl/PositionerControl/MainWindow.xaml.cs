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
        }

        // Field


        // Property
        private PositionerParams DmdPositionerParams { set; get; }
        private InstrDrvUtility DriverUtility { set; get; }
        private DmdPositionerSuite DmdPositioner { set; get; }
        
        // Method
        private void InitializeDmdPositioner(string visaAddress)
        {
            DmdPositionerParams = (PositionerParams)this.FindResource("positionerParams");
            DriverUtility = (InstrDrvUtility)this.FindResource("instrDrvUtility");
            DmdPositioner = (DmdPositionerSuite)this.FindResource("dmdPositionerSuite");    
            


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
    }    
}
