﻿using System;
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
using System.ComponentModel;
using System.Runtime.CompilerServices; 
using ETSL.TcpSocket;

namespace DoorSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Field
        

        // Constructor
        public MainWindow()
        {
            InitializeComponent();

            // TcpSocketServer
            TcpSvr = (TcpSocketServer)this.FindResource("tcpSvr");
            TcpSvr.ServerPort = 9001;

            // Simulator Settings
            SimParams = (DoorSimulatorParams)this.TryFindResource("simParams");            
            RxMsg = SimParams.RxMsg;
            TxMsg = SimParams.TxMsg;
            Cycle_ms = SimParams.Cycle_ms;  
            
            // Door1 State
            if(this.radioBtnDoor1Open.IsChecked == true && this.radioBtnDoor1Closed.IsChecked == false)
            {
                IsDoor1Closed = false;
            }
            else if(this.radioBtnDoor1Open.IsChecked==false && this.radioBtnDoor1Closed.IsChecked == true)
            {
                IsDoor1Closed = true;
            }
            else { IsDoor1Closed = false; }

            // Door2 State
            if (this.radioBtnDoor2Open.IsChecked == true && this.radioBtnDoor2Closed.IsChecked == false)
            {
                IsDoor2Closed = false;
            }
            else if (this.radioBtnDoor2Open.IsChecked == false && this.radioBtnDoor2Closed.IsChecked == true)
            {
                IsDoor2Closed = true;
            }
            else { IsDoor2Closed = false; }

            // DirtyFlag
            DirtyFlag = false;
        }

        // Property
        public TcpSocketServer TcpSvr { get; private set; }
        public DoorSimulatorParams SimParams { get; private set; }

        private string RxMsg { set; get; }
        private string TxMsg { set; get; }
        private int Cycle_ms { set; get; }

        private bool IsDoor1Closed { set; get; }
        private bool IsDoor2Closed { set; get; }
        private bool DirtyFlag { set; get; }
        
        
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

        public string ProcessReceivingMessage(string recString)
        {
            RxMsg = RxMsg.TrimEnd();
            if(recString == RxMsg)
            {
                return TxMsg;
            }
            else
            {
                return string.Empty;
            }
        }
        #endregion 

        // Commands
        private void SimSettingOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(SimParams==null || TcpSvr==null)
            {
                e.CanExecute = false;
            }
            else
            {
                if (TxMsg != SimParams.TxMsg ||
                    RxMsg != SimParams.RxMsg ||
                    Cycle_ms != SimParams.Cycle_ms ||
                    TcpSvr.ServerPort != SimParams.PortNum)
                {
                    DirtyFlag = true;
                    e.CanExecute = true;
                }
                else
                {
                    DirtyFlag = false;
                    e.CanExecute = false;
                }                   
            }
            e.Handled = true;
        }

        private void SimSettingOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if(TcpSvr.ServerState!=EnumServerState.ServerStopped)
            {
                TcpSvr.Stop();
            }
            
            TxMsg = SimParams.TxMsg;
            RxMsg = SimParams.RxMsg;
            Cycle_ms = SimParams.Cycle_ms;
            TcpSvr.ServerPort = SimParams.PortNum;
            TcpSvr.QueryTimeout_ms = 200;
            this.expdr.IsExpanded = false;
            DirtyFlag = false;

            e.Handled = true;
        }

        // EventHandler
        private void RadioButtonDoor1_Checked(object sender, RoutedEventArgs e)
        {
            if(radioBtnDoor1Open.IsChecked==true)
            {
                IsDoor1Closed = false;
            } 

            if(radioBtnDoor1Closed.IsChecked==true)
            {
                IsDoor1Closed = true;
            }
        }

        private void RadioButtonDoor2_Checked(object sender, RoutedEventArgs e)
        {
            if (radioBtnDoor2Open.IsChecked == true)
            {
                IsDoor2Closed = false;
            }

            if (radioBtnDoor2Closed.IsChecked == true)
            {
                IsDoor2Closed = true;
            }
        }

        private async void BtnRun_Click(object sender, RoutedEventArgs e)
        {
            TcpSvr.ProcessMessage = ProcessReceivingMessage;
            await TcpSvr.Start();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {  
            TcpSvr.Stop();            
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if(TcpSvr.ServerState!=EnumServerState.ServerStopped)
            {
                TcpSvr.Stop();
            }
        }

        private void expdr_Collapsed(object sender, RoutedEventArgs e)
        {
            this.expdrText.Content = "Press here to show more settings...";
            if (DirtyFlag)
            {
                SimParams.TxMsg = TxMsg;
                SimParams.RxMsg = RxMsg;
                SimParams.Cycle_ms = Cycle_ms;
                SimParams.PortNum = TcpSvr.ServerPort;
                DirtyFlag = false;
            }
        }

        private void expdr_Expanded(object sender, RoutedEventArgs e)
        {
            this.expdrText.Content = "Press here to switch back...";            
        }
    }
}
