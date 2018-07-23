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

        private bool IsCmdCanExecute(EnumPositionerType type)
        {
            bool cmdCanExecute = false;
            switch(type)
            {
                case EnumPositionerType.Slide:
                    if (DmdPositioner!=null && DmdPositioner.Initialized_Slide && DmdPositionerParams.IsMovingStop_Slide)
                    {
                        cmdCanExecute = true;
                    }
                    else
                    {
                        cmdCanExecute = false;
                    }
                    break;
                case EnumPositionerType.Lift:
                    if (DmdPositioner != null && DmdPositioner.Initialized_Lift && DmdPositionerParams.IsMovingStop_Lift)
                    {
                        cmdCanExecute = true;
                    }
                    else
                    {
                        cmdCanExecute = false;
                    }
                    break;
                case EnumPositionerType.Turntable:
                    if (DmdPositioner != null && DmdPositioner.Initialized_Turntable && DmdPositionerParams.IsMovingStop_Turntable)
                    {
                        cmdCanExecute = true;
                    }
                    else
                    {
                        cmdCanExecute = false;
                    }
                    break;
            }
            return cmdCanExecute;
        }

        // ------------ Commands(CanExecute) ------------
        // <1> Position querying commands
        private void QueryPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {            
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void QueryPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void QueryPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <2> Absolute position querying commands
        private void SeekAbsPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <3> Relative position querying commands
        private void SeekRelPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void SeekRelPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void SeekRelPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsCmdCanExecute(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <4> Apply Settings commands
        private void ApplySettingsCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool cmdCanExec = false;
            if(IsCmdCanExecute(EnumPositionerType.Slide))
            {
                int speed = -99999;
                if (this.tboxSpeed_Slide.Text != string.Empty)
                {
                    speed = Convert.ToInt32(this.tboxSpeed_Slide.Text);
                }

                double offset = 0;
                if (this.tboxOffset_Slide.Text != string.Empty)
                {
                    offset = Convert.ToDouble(this.tboxOffset_Slide.Text);
                }                

                if(DmdPositionerParams.CurrentSpeed_Slide != speed || DmdPositionerParams.Offset_Slide!=offset)
                {
                    cmdCanExec = true;
                }                 
            }

            e.CanExecute = cmdCanExec;
            e.Handled = true;
        }
        private void ApplySettingsCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool cmdCanExec = false;
            if (IsCmdCanExecute(EnumPositionerType.Lift))
            {
                int speed = -99999;
                if (this.tboxSpeed_Lift.Text != string.Empty)
                {
                    speed = Convert.ToInt32(this.tboxSpeed_Lift.Text);
                }

                double offset = 0;
                if (this.tboxOffset_Lift.Text != string.Empty)
                {
                    offset = Convert.ToDouble(this.tboxOffset_Lift.Text);
                }

                if (DmdPositionerParams.CurrentSpeed_Lift != speed || DmdPositionerParams.Offset_Lift != offset)
                {
                    cmdCanExec = true;
                }
            }

            e.CanExecute = cmdCanExec;
            e.Handled = true;
        }
        private void ApplySettingsCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool cmdCanExec = false;
            if (IsCmdCanExecute(EnumPositionerType.Turntable))
            {
                int speed = -99999;
                if (this.tboxSpeed_Turntable.Text != string.Empty)
                {
                    speed = Convert.ToInt32(this.tboxSpeed_Turntable.Text);
                }

                double offset = 0;
                if (this.tboxOffset_Turntable.Text != string.Empty)
                {
                    offset = Convert.ToDouble(this.tboxOffset_Turntable.Text);
                }

                if (DmdPositionerParams.CurrentSpeed_Turntable != speed || DmdPositionerParams.Offset_Turntable != offset)
                {
                    cmdCanExec = true;
                }
            }

            e.CanExecute = cmdCanExec;
            e.Handled = true;
        }

        // <5> Stop moving commands
        private void StopCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(DmdPositioner != null && DmdPositioner.HasInitialized && DmdPositioner.Initialized_Slide)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }
        private void StopCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DmdPositioner != null && DmdPositioner.HasInitialized && DmdPositioner.Initialized_Lift)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }
        private void StopCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DmdPositioner != null && DmdPositioner.HasInitialized && DmdPositioner.Initialized_Turntable)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        // ------------ Commands(Executed) ------------
        // <1> Position querying commands
        private void QueryPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {            

            e.Handled = true;
        }
        private void QueryPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void QueryPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        // <2> Absolute position querying commands
        private void SeekAbsPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void SeekAbsPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void SeekAbsPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        // <3> Relative position querying commands
        private void SeekRelPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void SeekRelPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void SeekRelPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        // <4> Apply Settings commands
        private void ApplySettingsCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void ApplySettingsCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void ApplySettingsCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        // <5> Stop moving commands
        private void StopCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void StopCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
        private void StopCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {

            e.Handled = true;
        }
    }
}
