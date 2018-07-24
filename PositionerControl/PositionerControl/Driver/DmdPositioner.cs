using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ETSL.InstrDriver.Base;

namespace ETSL.InstrDriver
{
    public class DmdPositionerBase/*: INotifyPropertyChanged*/
    {
        // Constructor
        public DmdPositionerBase(DmdPositionerSuite positioner)
        {
            dmdPosSuite = positioner;
        }

        // Field
        protected DmdPositionerSuite dmdPosSuite;        

        // Property
        protected string Command_Home { set; get; }
        protected string Command_SeekPosition { set; get; }
        protected string Command_SeekPositionRelative { set; get; }
        protected string Command_QueryPosition { set; get; }
        protected string Command_QueryOPC { set; get; }
        protected string Command_Stop { set; get; }
        protected string Command_SetSpeed { set; get; }
        //protected string Command_QuerySpeed { set; get; }  

        public bool OperationComplete
        {
            get
            {
                return GetOpcState();
            }
        }        
        
        public double GetCurrentPosition()
        {
            double pos = -99999;
            dmdPosSuite.SendCommand(Command_QueryPosition);
            Thread.Sleep(50);
            string resp = dmdPosSuite.ReadCommand();
            if (resp != string.Empty && resp.Contains("CP"))
            {
                string[] tmp = resp.Split(new string[] { " " }, StringSplitOptions.None);                
                pos = Convert.ToDouble(tmp[1]);                
            }
            return pos;
            
        }

        protected bool GetOpcState()
        {
            bool retVal = false;
            dmdPosSuite.SendCommand(Command_QueryOPC);
            Thread.Sleep(500);
            string resp = dmdPosSuite.ReadCommand();
            Int16 stateVal = Convert.ToInt16(resp);
            retVal = stateVal == 1 ? true : false;
            return retVal;
        }

        // Public Method
        public void Home()
        {
            dmdPosSuite.SendCommand(Command_Home);
        }

        public void Stop()
        {
            dmdPosSuite.SendCommand(Command_Stop);
        }

        public void SeekPosition(double targetPosition)
        {
            dmdPosSuite.SendCommand(Command_SeekPosition + " " + targetPosition.ToString());
        }

        public void SeekPositionRelative(double targetPosition)
        {
            dmdPosSuite.SendCommand(Command_SeekPositionRelative + " " + targetPosition.ToString());
        }

        public void SetSpeed(int speed)
        {
            dmdPosSuite.SendCommand(Command_SetSpeed + " " + speed.ToString());
        }
    }

    public class DmdPositionerSuite: VisaInstrDriver
    {
        // ====== Nested class member ======
        public class DmdSlide: DmdPositionerBase
        {
            // Constructor
            public DmdSlide(DmdPositionerSuite positioner)
                :base(positioner)
            {
                Command_Home = "AXIS1:HOME";
                Command_Stop = "AXIS1:ST";
                Command_SeekPosition = "AXIS1:SK";
                Command_SeekPositionRelative = "AXIS1:SKR";
                Command_QueryPosition = "AXIS1:CP?";
                Command_QueryOPC = "AXIS1:*OPC?";
                Command_SetSpeed = "AXIS1:S";                
            }
        }

        public class DmdLift : DmdPositionerBase
        {
            // Constructor
            public DmdLift(DmdPositionerSuite positioner)
                : base(positioner)
            {
                Command_Home = "AXIS2:HOME";
                Command_Stop = "AXIS2:ST";
                Command_SeekPosition = "AXIS2:SK";
                Command_SeekPositionRelative = "AXIS2:SKR";
                Command_QueryPosition = "AXIS2:CP?";
                Command_QueryOPC = "AXIS2:*OPC?";
                Command_SetSpeed = "AXIS2:S";                
            }

        }

        public class DmdTurntable : DmdPositionerBase
        {
            // Constructor
            public DmdTurntable(DmdPositionerSuite positioner)
                : base(positioner)
            {
                Command_Home = "AXIS3:HOME";
                Command_Stop = "AXIS3:ST";
                Command_SeekPosition = "AXIS3:SK";
                Command_SeekPositionRelative = "AXIS3:SKR";
                Command_QueryPosition = "AXIS3:CP?";
                Command_QueryOPC = "AXIS3:*OPC?";
                Command_SetSpeed = "AXIS3:S";                
            }
        }

        // Constructor
        public DmdPositionerSuite()
        {
            this.slide     = null;
            this.lift      = null;
            this.turntable = null; 
        }

        public DmdPositionerSuite(string visaAddr)
        {            
            VisaAddress = visaAddr;            
            this.slide = null;
        }

        // Field
        //private VisaInstrDriver instrDrv;
        private DmdSlide     slide;
        private DmdLift      lift;
        private DmdTurntable turntable;

        private string id_Slide     = string.Empty;
        private string id_Lift      = string.Empty;
        private string id_Turntable = string.Empty;

        private bool initialized_Slide     = false;
        private bool initialized_Lift      = false;
        private bool initialized_Turntable = false;

        // Property
        public DmdSlide Slide
        {
            get
            {
                if(Initialized_Slide)
                {
                    slide = new DmdSlide(this);
                }
                return this.slide;
            }
        }
        public DmdLift Lift
        {
            get
            {
                if (Initialized_Lift)
                {
                    lift = new DmdLift(this);
                }
                return this.lift;
            }
        }
        public DmdTurntable Turntable
        {
            get
            {
                if (Initialized_Turntable)
                {
                    turntable = new DmdTurntable(this);
                }
                return this.turntable;
            }
        }
        public bool Initialized_Slide
        {
            get
            {
                return this.initialized_Slide;
            }
            set
            {
                this.initialized_Slide = value;
                NotifyPropertyChanged("Initialized_Slide");
            }
        }
        public bool Initialized_Lift
        {
            get
            {
                return this.initialized_Lift;
            }
            set
            {
                this.initialized_Lift = value;
                NotifyPropertyChanged("Initialized_Lift");
            }
        }
        public bool Initialized_Turntable
        {
            get
            {
                return this.initialized_Turntable;
            }
            set
            {
                this.initialized_Turntable = value;
                NotifyPropertyChanged("Initialized_Turntable");
            }
        }


        // Method
        public override bool Initialize()
        {
            if(base.Initialize())
            {
                //Slide
                SendCommand("AXIS1:*IDN?");
                Thread.Sleep(200);
                this.id_Slide = ReadCommand();
                if (this.id_Slide.Contains("ETS-Lindgren"))
                {
                    Initialized_Slide = true;
                }
                else
                {
                    MessageBox.Show("Failed to initialize slide!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Initialized_Slide = false;
                }

                //Lift
                SendCommand("AXIS2:*IDN?");
                Thread.Sleep(200);
                this.id_Lift = ReadCommand();
                if (this.id_Lift.Contains("ETS-Lindgren"))
                {
                    Initialized_Lift = true;
                }
                else
                {
                    MessageBox.Show("Failed to initialize lift!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Initialized_Lift = false;
                }

                //Turntable
                SendCommand("AXIS3:*IDN?");
                Thread.Sleep(200);
                this.id_Turntable = ReadCommand();
                if (this.id_Turntable.Contains("ETS-Lindgren"))
                {
                    Initialized_Turntable = true;
                }
                else
                {
                    MessageBox.Show("Failed to initialize turntable!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Initialized_Turntable = false;
                }

                if (Initialized_Slide && Initialized_Lift && Initialized_Turntable)
                {
                    InstrID = id_Slide;
                    HasInitialized = true;                    
                }
                else
                {
                    InstrID = string.Empty;
                    HasInitialized = false;                    
                }
            }
            else
            {
                InstrID = string.Empty;
                HasInitialized = false;
            }

            return HasInitialized;
        }

        public override void DeInitialize()
        {
            base.DeInitialize();

            Initialized_Slide = false;
            Initialized_Lift = false;
            Initialized_Turntable = false;

            this.slide = null;
            this.lift = null;
            this.turntable = null;
        }
    }    
}
