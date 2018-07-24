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
using System.Text.RegularExpressions;
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

            // ---------- Positioner Initialization ---------- 
            DmdPositionerParams = (PositionerParams)this.FindResource("positionerParams");
            DriverUtility = (InstrDrvUtility)this.FindResource("instrDrvUtility");
            DmdPositioner = (DmdPositionerSuite)this.FindResource("dmdPositionerSuite");

            // Positioner VISA Address Setting
            DmdPositioner.VisaAddress = DmdPositionerParams.VisaAddress;
            if (this.cbVisaAddrList.Items.IsEmpty)
            {
                this.cbVisaAddrList.Items.Add(DmdPositioner.VisaAddress);
            }
            this.cbVisaAddrList.SelectedIndex = 0;

            // Initialization Operation
            InitializeDmdPositioner();
        }

        // Field
        static private object locker = new object();

        // Property
        private PositionerParams DmdPositionerParams { set; get; }
        private InstrDrvUtility DriverUtility { set; get; }
        private DmdPositionerSuite DmdPositioner { set; get; }

        private bool ForceStop_Slide { set; get; }
        private bool ForceStop_Lift { set; get; }
        private bool ForceStop_Turntable { set; get; }

        // Method
        private void InitializeDmdPositioner()
        {
            // Init
            DmdPositioner.Initialize();
            Thread.Sleep(200);
            this.tblockStatus.Text = DmdPositioner.InstrID;

            // Query current position for Slide, Lift and Turntable and update UI accordingly.
            DmdPositionerParams.CurrentPosition_Slide     = DmdPositioner.Slide.GetCurrentPosition();
            DmdPositionerParams.CurrentPosition_Lift      = DmdPositioner.Lift.GetCurrentPosition();
            DmdPositionerParams.CurrentPosition_Turntable = DmdPositioner.Turntable.GetCurrentPosition();

            // Set Speed 
            DmdPositioner.Slide.SetSpeed(DmdPositionerParams.Speed_Slide);
            DmdPositioner.Lift.SetSpeed(DmdPositionerParams.Speed_Lift);
            DmdPositioner.Turntable.SetSpeed(DmdPositionerParams.Speed_Turntable);

            // Home Position Checkbox
            this.chkboxHomePos_Slide.IsChecked = false;
            this.chkboxHomePos_Lift.IsChecked = false;
            this.chkboxHomePos_Turntable.IsChecked = false;

            // Target Absolute Position
            this.tboxTargetAbsPos_Slide.Text     = "0.0";
            this.tboxTargetAbsPos_Lift.Text      = "0.0";
            this.tboxTargetAbsPos_Turntable.Text = "0.0";

            // Direction Radio Button
            this.radBtnPosDir_Slide.IsChecked = true;
            this.radBtnNegDir_Slide.IsChecked = false;
            this.radBtnPosDir_Lift.IsChecked = true;
            this.radBtnNegDir_Lift.IsChecked = false;
            this.radBtnPosDir_Turntable.IsChecked = true;
            this.radBtnNegDir_Turntable.IsChecked = false;

            // Target Relative Position
            this.tboxTargetRelPos_Slide.Text = "0.0";
            this.tboxTargetRelPos_Lift.Text = "0.0";
            this.tboxTargetRelPos_Turntable.Text = "0.0";

            ForceStop_Slide = false;
            ForceStop_Lift = false;
            ForceStop_Turntable = false;
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
        private void ReConnect_Click(object sender, RoutedEventArgs e)
        {
            DmdPositioner.DeInitialize();
            Thread.Sleep(200);
            this.tblockStatus.Text = DmdPositioner.InstrID;

            // Open Positioner Suite            
            DmdPositioner.VisaAddress = DmdPositionerParams.VisaAddress = this.cbVisaAddrList.SelectedItem.ToString();
            InitializeDmdPositioner();
        }

        private bool IsInitializedAndStopped(EnumPositionerType type)
        {
            bool cmdCanExecute = false;
            switch(type)
            {
                case EnumPositionerType.Slide:
                    if (DmdPositioner!=null && DmdPositioner.Initialized_Slide && DmdPositionerParams.IsStopped_Slide)
                    {
                        cmdCanExecute = true;
                    }
                    else
                    {
                        cmdCanExecute = false;
                    }
                    break;
                case EnumPositionerType.Lift:
                    if (DmdPositioner != null && DmdPositioner.Initialized_Lift && DmdPositionerParams.IsStopped_Lift)
                    {
                        cmdCanExecute = true;
                    }
                    else
                    {
                        cmdCanExecute = false;
                    }
                    break;
                case EnumPositionerType.Turntable:
                    if (DmdPositioner != null && DmdPositioner.Initialized_Turntable && DmdPositionerParams.IsStopped_Turntable)
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
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void QueryPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void QueryPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <2> Absolute position querying commands
        private void SeekAbsPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {            
            if (IsInitializedAndStopped(EnumPositionerType.Slide) && this.tboxTargetAbsPos_Slide.Text!=string.Empty)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {            
            if (IsInitializedAndStopped(EnumPositionerType.Lift) && this.tboxTargetAbsPos_Lift.Text!=string.Empty)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //Regex re = new Regex("[^0-9.-]+");
            //bool flag = re.IsMatch(this.tboxTargetAbsPos_Turntable.Text);
            if (IsInitializedAndStopped(EnumPositionerType.Turntable) && this.tboxTargetAbsPos_Turntable.Text!=string.Empty)               
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            e.Handled = true;
        }

        // <3> Relative position querying commands
        private void SeekRelPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void SeekRelPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void SeekRelPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <4> Apply Settings commands
        private void ApplySettingsCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            bool cmdCanExec = false;
            if(IsInitializedAndStopped(EnumPositionerType.Slide))
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

                if(DmdPositionerParams.Speed_Slide != speed || DmdPositionerParams.Offset_Slide!=offset)
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
            if (IsInitializedAndStopped(EnumPositionerType.Lift))
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

                if (DmdPositionerParams.Speed_Lift != speed || DmdPositionerParams.Offset_Lift != offset)
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
            if (IsInitializedAndStopped(EnumPositionerType.Turntable))
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

                if (DmdPositionerParams.Speed_Turntable != speed || DmdPositionerParams.Offset_Turntable != offset)
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
        private async void QueryPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => UpdateCurrentPositionUI(EnumPositionerType.Slide));
            e.Handled = true;
        }
        private async void QueryPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => UpdateCurrentPositionUI(EnumPositionerType.Lift));
            e.Handled = true;
        }
        private async void QueryPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => UpdateCurrentPositionUI(EnumPositionerType.Turntable));
            e.Handled = true;
        }

        // <2> Absolute position querying commands
        private async void SeekAbsPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Target Value
            double pos = Convert.ToDouble(this.tboxTargetAbsPos_Slide.Text);
            pos = Math.Round(pos, 1);
            DmdPositionerParams.TargetAbsolutePosition_Slide = pos;
            this.tboxTargetAbsPos_Slide.Text = pos.ToString();

            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Slide));
            e.Handled = true;
        }
        private async void SeekAbsPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Target Value
            double pos = Convert.ToDouble(this.tboxTargetAbsPos_Lift.Text);
            pos = Math.Round(pos, 1);
            DmdPositionerParams.TargetAbsolutePosition_Lift = pos;
            this.tboxTargetAbsPos_Lift.Text = pos.ToString();

            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Lift));
            e.Handled = true;
        }
        private async void SeekAbsPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {  
            // Target Value 
            double pos = Convert.ToDouble(this.tboxTargetAbsPos_Turntable.Text);
            pos = Math.Round(pos, 1);
            DmdPositionerParams.TargetAbsolutePosition_Turntable = pos;
            this.tboxTargetAbsPos_Turntable.Text = pos.ToString();                        

            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Turntable));
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
            lock (locker)
            {
                ForceStop_Slide = true;
            }
            //await Task.Run(() => Stop(EnumPositionerType.Slide));
            e.Handled = true;
        }
        private void StopCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            lock (locker)
            {
                ForceStop_Lift = true;
            }
            //await Task.Run(() => Stop(EnumPositionerType.Lift));
            e.Handled = true;
        }
        private void StopCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            lock(locker)
            {
                ForceStop_Turntable = true;
            }            
            //await Task.Run(() => Stop(EnumPositionerType.Turntable));
            e.Handled = true;
        }

        private void chkboxHomePos_Slide_Checked(object sender, RoutedEventArgs e)
        {
            if(this.chkboxHomePos_Slide.IsChecked == true)
            {
                this.tboxTargetAbsPos_Slide.Text = "0.0";
                this.tboxTargetAbsPos_Slide.IsEnabled = false;
            }          
        }

        private void chkboxHomePos_Lift_Checked(object sender, RoutedEventArgs e)
        {
            if (this.chkboxHomePos_Lift.IsChecked == true)
            {
                this.tboxTargetAbsPos_Lift.Text = "0.0";
                this.tboxTargetAbsPos_Lift.IsEnabled = false;
            }
        }

        private void chkboxHomePos_Turntable_Checked(object sender, RoutedEventArgs e)
        {
            if (this.chkboxHomePos_Turntable.IsChecked == true)
            {
                this.tboxTargetAbsPos_Turntable.Text = "0.0";
                this.tboxTargetAbsPos_Turntable.IsEnabled = false;
            }
        }

        private void chkboxHomePos_Slide_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.chkboxHomePos_Slide.IsChecked == false)
            {               
                this.tboxTargetAbsPos_Slide.IsEnabled = true;
            }
        }

        private void chkboxHomePos_Lift_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.chkboxHomePos_Lift.IsChecked == false)
            {
                this.tboxTargetAbsPos_Lift.IsEnabled = true;
            }
        }

        private void chkboxHomePos_Turntable_Unchecked(object sender, RoutedEventArgs e)
        {
            if (this.chkboxHomePos_Turntable.IsChecked == false)
            {
                this.tboxTargetAbsPos_Turntable.IsEnabled = true;
            }
        }

        private void radBtnPosDir_Slide_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Slide = true;
        }

        private void radBtnNegDir_Slide_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Slide = false;
        }

        private void radBtnPosDir_Lift_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Lift = true;
        }

        private void radBtnNegDir_Lift_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Lift = false;
        }

        private void radBtnPosDir_Turntable_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Turntable = true;
        }

        private void radBtnNegDir_Turntable_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetRelativeDirection_Turntable = false;
        }

        private void UpdateCurrentPositionUI(EnumPositionerType type)
        {            
            switch (type)
            {
                case EnumPositionerType.Slide:
                    DmdPositionerParams.CurrentPosition_Slide = DmdPositioner.Slide.GetCurrentPosition();
                    break;
                case EnumPositionerType.Lift:
                    DmdPositionerParams.CurrentPosition_Lift = DmdPositioner.Lift.GetCurrentPosition();
                    break;
                case EnumPositionerType.Turntable:
                    DmdPositionerParams.CurrentPosition_Turntable = DmdPositioner.Turntable.GetCurrentPosition();
                    break;
            }
        }

        private void SeekAbsolutePosition(EnumPositionerType type)
        {
            double absPos = 0;
            bool opcFlag = false;
            bool forceStop = false;
            DmdPositionerBase equipment = null;

            switch (type)
            {
                case EnumPositionerType.Slide:
                    absPos = DmdPositionerParams.TargetAbsolutePosition_Slide;
                    equipment = DmdPositioner.Slide;                    
                    //DmdPositionerParams.IsStopped_Slide = false;
                    break;
                case EnumPositionerType.Lift:
                    absPos = DmdPositionerParams.TargetAbsolutePosition_Lift;
                    equipment = DmdPositioner.Lift;
                    //DmdPositionerParams.IsStopped_Lift = false;
                    break;
                case EnumPositionerType.Turntable:
                    absPos = DmdPositionerParams.TargetAbsolutePosition_Turntable;
                    equipment = DmdPositioner.Turntable;
                    //DmdPositionerParams.IsStopped_Turntable= false;
                    break;                
            }

            // Seek Position command
            equipment.SeekPosition(absPos);
            Thread.Sleep(200);

            // Querying loop for operation complete
            while(true)
            {
                // OPC Flag
                opcFlag = equipment.OperationComplete;

                lock(locker)
                {
                    // Force Stop Flag
                    if (type == EnumPositionerType.Slide)
                    {
                        forceStop = ForceStop_Slide;
                    }
                    else if (type == EnumPositionerType.Lift)
                    {
                        forceStop = ForceStop_Lift;
                    }
                    else
                    {
                        forceStop = ForceStop_Turntable;
                    }
                }                

                if(opcFlag || forceStop)
                {
                    break;
                }
                else
                {
                    UpdateCurrentPositionUI(type);
                    Thread.Sleep(100);
                }
            }

            if(forceStop)
            {
                lock(locker)
                {
                    if (type == EnumPositionerType.Slide)
                    {
                        ForceStop_Slide = false;
                    }
                    else if (type == EnumPositionerType.Lift)
                    {
                        ForceStop_Lift = false;
                    }
                    else
                    {
                        ForceStop_Turntable = false;
                    }
                }               

                equipment.Stop();
                Thread.Sleep(200);
            }

            UpdateCurrentPositionUI(type);
        }

        private void SeekRelativePosition(EnumPositionerType type)
        {            
            double relPos = 0;                      
            bool opcFlag = false;
            DmdPositionerBase equipment = null;

            switch (type)
            {
                case EnumPositionerType.Slide:
                    relPos = DmdPositionerParams.TargetRelativePosition_Slide;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Slide ? relPos : -1 * relPos;                        
                    equipment = DmdPositioner.Slide;
                    //DmdPositionerParams.IsStopped_Slide = false;
                    break;
                case EnumPositionerType.Lift:
                    relPos = DmdPositionerParams.TargetRelativePosition_Lift;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Lift ? relPos : -1 * relPos;
                    equipment = DmdPositioner.Lift;
                    //DmdPositionerParams.IsStopped_Lift = false;
                    break;
                case EnumPositionerType.Turntable:
                    relPos = DmdPositionerParams.TargetRelativePosition_Turntable;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Turntable? relPos : -1 * relPos;
                    equipment = DmdPositioner.Turntable;
                    //DmdPositionerParams.IsStopped_Turntable = false;
                    break;
            }

            // Seek Position command           
            equipment.SeekPositionRelative(relPos);

            // Querying loop for operation complete
            while (true)
            {
                opcFlag = equipment.OperationComplete;
                if (opcFlag)
                {
                    break;
                }
                else
                {
                    UpdateCurrentPositionUI(type);
                    Thread.Sleep(100);
                }
            }

            UpdateCurrentPositionUI(type);

            //switch (type)
            //{
            //    case EnumPositionerType.Slide:
            //        DmdPositionerParams.IsStopped_Slide = true;
            //        break;
            //    case EnumPositionerType.Lift:
            //        DmdPositionerParams.IsStopped_Lift = true;
            //        break;
            //    case EnumPositionerType.Turntable:
            //        DmdPositionerParams.IsStopped_Turntable = true;
            //        break;
            //}
        }

        private void Stop(EnumPositionerType type)
        {
            switch (type)
            {
                case EnumPositionerType.Slide:
                    DmdPositioner.Slide.Stop();
                    break;
                case EnumPositionerType.Lift:
                    DmdPositioner.Lift.Stop();
                    break;
                case EnumPositionerType.Turntable:
                    DmdPositioner.Turntable.Stop();
                    break;
            }

            UpdateCurrentPositionUI(type);
        }        
    }
}
