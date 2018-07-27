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

        private bool ChangeSpeed_Slide { set; get; }
        private bool ChangeSpeed_Lift { set; get; }
        private bool ChangeSpeed_Turntable { set; get; }

        private bool ChangeOffset_Slide { set; get; }
        private bool ChangeOffset_Lift { set; get; }
        private bool ChangeOffset_Turntable { set; get; }

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
            
            int speed = Convert.ToInt16(DmdPositionerParams.Speed_Slide);
            DmdPositioner.Slide.SetSpeed(speed);
            speed = Convert.ToInt16(DmdPositionerParams.Speed_Lift);
            DmdPositioner.Lift.SetSpeed(speed);
            speed = Convert.ToInt16(DmdPositionerParams.Speed_Turntable);
            DmdPositioner.Turntable.SetSpeed(speed);

            ForceStop_Slide = false;
            ForceStop_Lift = false;
            ForceStop_Turntable = false;

            ChangeSpeed_Slide = false;
            ChangeSpeed_Lift = false;
            ChangeSpeed_Turntable = false;

            ChangeOffset_Slide = false;
            ChangeOffset_Lift = false;
            ChangeOffset_Turntable = false;
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

        // <2> Absolute position seeking commands
        private void SeekAbsPosCmd_Slide_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {            
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Slide);
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Lift_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Lift);
            e.Handled = true;
        }
        private void SeekAbsPosCmd_Turntable_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsInitializedAndStopped(EnumPositionerType.Turntable);
            e.Handled = true;
        }

        // <3> Relative position seeking commands
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
                if (this.tboxSpeed_Slide.Text != string.Empty)
                {                    
                    if(DmdPositionerParams.Speed_Slide.ToString() != this.tboxSpeed_Slide.Text)
                    { 
                        ChangeSpeed_Slide = true;
                    }
                }
                
                if (this.tboxOffset_Slide.Text != string.Empty)
                {                    
                    if(DmdPositionerParams.Offset_Slide.ToString() != this.tboxOffset_Slide.Text)
                    {
                        ChangeOffset_Slide = true;
                    }
                }                

                if( ChangeSpeed_Slide || ChangeOffset_Slide )
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
                if (this.tboxSpeed_Lift.Text != string.Empty)
                {
                    if (DmdPositionerParams.Speed_Lift.ToString() != this.tboxSpeed_Lift.Text)
                    {
                        ChangeSpeed_Lift = true;
                    }
                }

                if (this.tboxOffset_Lift.Text != string.Empty)
                {
                    if (DmdPositionerParams.Offset_Lift.ToString() != this.tboxOffset_Lift.Text)
                    {
                        ChangeOffset_Lift = true;
                    }
                }

                if (ChangeSpeed_Lift || ChangeOffset_Lift)
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
                if (this.tboxSpeed_Turntable.Text != string.Empty)
                {
                    if (DmdPositionerParams.Speed_Turntable.ToString() != this.tboxSpeed_Turntable.Text)
                    {
                        ChangeSpeed_Turntable = true;
                    }
                }

                if (this.tboxOffset_Turntable.Text != string.Empty)
                {
                    if (DmdPositionerParams.Offset_Turntable.ToString() != this.tboxOffset_Turntable.Text)
                    {
                        ChangeOffset_Turntable = true;
                    }
                }

                if (ChangeSpeed_Turntable || ChangeOffset_Turntable)
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

        // <2> Absolute position querying commands5
        private async void SeekAbsPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {            
            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Slide));
            this.tboxTargetAbsPos_Slide.Focus();
            e.Handled = true;
        }
        private async void SeekAbsPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Lift));
            this.tboxTargetAbsPos_Lift.Focus();
            e.Handled = true;
        }
        private async void SeekAbsPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => SeekAbsolutePosition(EnumPositionerType.Turntable));
            this.tboxTargetAbsPos_Turntable.Focus();
            e.Handled = true;
        }

        // <3> Relative position querying commands
        private async void SeekRelPosCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => SeekRelativePosition(EnumPositionerType.Slide));
            this.tboxTargetRelPos_Slide.Focus();
            e.Handled = true;
        }
        private async void SeekRelPosCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => SeekRelativePosition(EnumPositionerType.Lift));
            this.tboxTargetRelPos_Lift.Focus();
            e.Handled = true;
        }
        private async void SeekRelPosCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            await Task.Run(() => SeekRelativePosition(EnumPositionerType.Turntable));
            this.tboxTargetRelPos_Turntable.Focus();
            e.Handled = true;
        }

        // <4> Apply Settings commands
        private void ApplySettingsCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(ChangeSpeed_Slide)
            {
                int speed = Convert.ToInt16(this.tboxSpeed_Slide.Text);                 
                DmdPositioner.Slide.SetSpeed(speed);
                DmdPositionerParams.Speed_Slide = speed.ToString();
                ChangeSpeed_Slide = false;
            }
            if(ChangeOffset_Slide)
            {
                DmdPositionerParams.Offset_Slide = this.tboxOffset_Slide.Text;
                ChangeOffset_Slide = false;
            }

            e.Handled = true;
        }
        private void ApplySettingsCmd_Lift_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ChangeSpeed_Lift)
            {
                int speed = Convert.ToInt16(this.tboxSpeed_Lift.Text);
                DmdPositioner.Lift.SetSpeed(speed);
                DmdPositionerParams.Speed_Lift = speed.ToString();
                ChangeSpeed_Lift = false;
            }
            if (ChangeOffset_Lift)
            {
                DmdPositionerParams.Offset_Lift = this.tboxOffset_Lift.Text;
                ChangeOffset_Lift = false;
            }

            e.Handled = true;
        }
        private void ApplySettingsCmd_Turntable_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ChangeSpeed_Turntable)
            {
                int speed = Convert.ToInt16(this.tboxSpeed_Turntable.Text);
                DmdPositioner.Turntable.SetSpeed(speed);
                DmdPositionerParams.Speed_Turntable = speed.ToString();
                ChangeSpeed_Turntable = false;
            }
            if (ChangeOffset_Turntable)
            {
                DmdPositionerParams.Offset_Turntable = this.tboxOffset_Turntable.Text;
                ChangeOffset_Turntable = false;
            }

            e.Handled = true;
        }

        // <5> Stop moving commands
        private async void StopCmd_Slide_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(DmdPositionerParams.IsStopped_Slide)
            {
                await Task.Run(() => Stop(EnumPositionerType.Slide));
            }
            else
            {
                lock (locker)
                {
                    ForceStop_Slide = true;
                }
            }

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
                    DmdPositionerParams.IsStopped_Slide = false;
                    break;
                case EnumPositionerType.Lift:
                    absPos = DmdPositionerParams.TargetAbsolutePosition_Lift;
                    equipment = DmdPositioner.Lift;
                    DmdPositionerParams.IsStopped_Lift = false;
                    break;
                case EnumPositionerType.Turntable:
                    absPos = DmdPositionerParams.TargetAbsolutePosition_Turntable;
                    equipment = DmdPositioner.Turntable;
                    DmdPositionerParams.IsStopped_Turntable= false;
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
                    Thread.Sleep(50);
                }
            }

            Stop(type);            
        }

        private void SeekRelativePosition(EnumPositionerType type)
        {
            double relPos = 0;
            bool opcFlag = false;
            bool forceStop = false;
            DmdPositionerBase equipment = null;

            switch (type)
            {
                case EnumPositionerType.Slide:
                    relPos = DmdPositionerParams.TargetRelativePosition_Slide;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Slide ? relPos : relPos * (-1);
                    equipment = DmdPositioner.Slide;
                    DmdPositionerParams.IsStopped_Slide = false;
                    break;
                case EnumPositionerType.Lift:
                    relPos = DmdPositionerParams.TargetRelativePosition_Lift;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Lift ? relPos : relPos * (-1);
                    equipment = DmdPositioner.Lift;
                    DmdPositionerParams.IsStopped_Lift = false;
                    break;
                case EnumPositionerType.Turntable:
                    relPos = DmdPositionerParams.TargetRelativePosition_Turntable;
                    relPos = DmdPositionerParams.TargetRelativeDirection_Turntable ? relPos : relPos * (-1);
                    equipment = DmdPositioner.Turntable;
                    DmdPositionerParams.IsStopped_Turntable = false;
                    break;
            }

            // Seek Position command
            equipment.SeekPositionRelative(relPos);
            Thread.Sleep(200);

            // Querying loop for operation complete
            while (true)
            {
                // OPC Flag
                opcFlag = equipment.OperationComplete;

                lock (locker)
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

                if (opcFlag || forceStop)
                {
                    break;
                }
                else
                {
                    UpdateCurrentPositionUI(type);
                    Thread.Sleep(50);
                }
            }

            Stop(type);
        }

        private void Stop(EnumPositionerType type)
        {
            switch (type)
            {
                case EnumPositionerType.Slide:
                    DmdPositioner.Slide.Stop();
                    lock (locker)
                    {
                        ForceStop_Slide = false;
                        DmdPositionerParams.IsStopped_Slide = true;
                    }
                    break;
                case EnumPositionerType.Lift:
                    DmdPositioner.Lift.Stop();
                    lock (locker)
                    {
                        ForceStop_Lift = false;
                        DmdPositionerParams.IsStopped_Lift = true;
                    }
                    break;                
                
                case EnumPositionerType.Turntable:
                    DmdPositioner.Turntable.Stop();
                    lock (locker)
                    {
                        ForceStop_Turntable = false;
                        DmdPositionerParams.IsStopped_Turntable = true;
                    }
                    break; 
            }

            Thread.Sleep(50);
            UpdateCurrentPositionUI(type);
        }

        private void chkboxTargetHome_Slide_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetAbsolutePosition_Slide = 0;
        }

        private void chkboxTargetHome_Lift_Checked(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetAbsolutePosition_Lift = 0;
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            DmdPositionerParams.TargetAbsolutePosition_Turntable = 0;
        }
    }
}
