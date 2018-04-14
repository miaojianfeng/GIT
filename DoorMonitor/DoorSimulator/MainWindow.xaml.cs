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
        }

        // Property
        public TcpSocketServer TcpSvr { get; private set; }
        public DoorSimulatorParams SimParams { get; private set; }

        private string RxMsg { set; get; }
        private string TxMsg { set; get; }
        private int Cycle_ms { set; get; }

        private bool IsDoor1Closed { set; get; }
        private bool IsDoor2Closed { set; get; }
        
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
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = false;
                }                   
            }
            e.Handled = true;
        }

        private void SimSettingOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TxMsg = SimParams.TxMsg;
            RxMsg = SimParams.RxMsg;
            Cycle_ms = SimParams.Cycle_ms;
            TcpSvr.ServerPort = SimParams.PortNum;
            this.expdr.IsExpanded = false;
            e.Handled = true;
        }

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
    }

    public class DoorSimulatorParams
    {
        // Field
        private string txMsg = "Door1:Closed;Door2:Closed\n";
        private string rxMsg = "DoorState?\n";
        private int cycle_ms = 500;
        private UInt16 portNum = 9001;
        //private bool isDoor1Closed = true;
        //private bool isDoor2Closed = true;
        

        public DoorSimulatorParams()
        {

        }

        // Property   
        public string RxMsg
        {
            set
            {
                this.rxMsg = value;
                NotifyPropertyChanged("RxMsg");
            }
            get
            {
                return this.rxMsg;
            }
        }

        public string TxMsg
        {
            set
            {
                this.txMsg = value;
                NotifyPropertyChanged("TxMsg");
            }
            get
            {
                return this.txMsg;
            }
        }

        public int Cycle_ms
        {
            set
            {
                this.cycle_ms = value;
                NotifyPropertyChanged("Cycle_ms");
            }
            get
            {
                return this.cycle_ms;
            }
        }

        public UInt16 PortNum
        {
            set
            {
                this.portNum = value;
                NotifyPropertyChanged("PortNum");
            }
            get
            {
                return this.portNum;
            }
        }

        //public bool IsDoor1Closed
        //{
        //    set
        //    {
        //        this.isDoor1Closed = value;
        //        NotifyPropertyChanged("IsDoor1Closed");
        //    }
        //    get
        //    {
        //        return this.isDoor1Closed;
        //    }
        //}

        //public bool IsDoor2Closed
        //{
        //    set
        //    {
        //        this.isDoor2Closed = value;
        //        NotifyPropertyChanged("IsDoor2Closed");
        //    }
        //    get
        //    {
        //        return this.isDoor2Closed;
        //    }
        //}

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
        #endregion        
    }
}
