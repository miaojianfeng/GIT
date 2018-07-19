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

    public class DmdPositioner: VisaInstrDriver
    {
        // Nested class member
        public class DmdSlideWay
        {
            // Constructor
            public DmdSlideWay(DmdPositioner positioner)
            {
                dmdPos = positioner;
            }

            // Field
            DmdPositioner dmdPos;

            // Method
            public void Home()
            {
                dmdPos.SendCommand("AXIS1:HOME");
            }
        }

        // Constructor
        public DmdPositioner()
        {
            this.slideWay = null;
        }

        public DmdPositioner(string visaAddr)
        {            
            VisaAddress = visaAddr;            
            this.slideWay = null;
        }

        // Field
        //private VisaInstrDriver instrDrv;
        private DmdSlideWay slideWay;
        private string slideID = string.Empty;
        private bool slideInitialized = false;

        // Property
        public DmdSlideWay Slide
        {
            get
            {
                if(this.slideInitialized)
                {
                    slideWay = new DmdSlideWay(this);
                }
                return this.slideWay;
            }
        }

        // Method
        public override bool Initialize()
        {
            if(base.Initialize())
            {
                //SlideWay
                SendCommand("AXIS1:*IDN?");
                Thread.Sleep(200);
                this.slideID = ReadCommand();
                if (this.slideID.Contains("ETS-Lindgren"))
                {
                    this.slideInitialized = true;
                }
                else
                {
                    MessageBox.Show("Failed to initialize SlideWay!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.slideInitialized = false;
                }

                if (this.slideInitialized)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }    
}
