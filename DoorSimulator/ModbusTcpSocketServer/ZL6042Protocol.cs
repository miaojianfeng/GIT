using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETSL.TcpSocket
{
    public enum EnumDIState
    {
        LowLevel = 0,
        HighLevel = 1
    }

    public enum EnumDIStateChange
    {
        LowToHigh,
        HighToLow
    }

    public class ZL6042DISimulator
    {
        // Constructor
        public ZL6042DISimulator()
        {
            
        }

        // Field
        private EnumDIState diStaCh1 = EnumDIState.HighLevel;
        private EnumDIState diStaCh2 = EnumDIState.HighLevel;
        private EnumDIState diStaCh3 = EnumDIState.HighLevel;
        private EnumDIState diStaCh4 = EnumDIState.HighLevel;

        private static object thisLock = new object();

        // Property
        public EnumDIState DIStateCh1
        {
            set
            {
                lock(thisLock)
                {
                    diStaCh1 = value;
                }
            }
            get
            {
                lock(thisLock)
                {
                    return diStaCh1;
                }
            }
        }
        public EnumDIState DIStateCh2
        {
            set
            {
                lock (thisLock)
                {
                    diStaCh2 = value;
                }
            }
            get
            {
                lock (thisLock)
                {
                    return diStaCh2;
                }
            }
        }
        public EnumDIState DIStateCh3
        {
            set
            {
                lock (thisLock)
                {
                    diStaCh3 = value;
                }
            }
            get
            {
                lock (thisLock)
                {
                    return diStaCh3;
                }
            }
        }
        public EnumDIState DIStateCh4
        {
            set
            {
                lock (thisLock)
                {
                    diStaCh4 = value;
                }
            }
            get
            {
                lock (thisLock)
                {
                    return diStaCh4;
                }
            }
        }


        // Method
        public string ProcessDIStateQueryMessage(string diStaQueryMessage)
        {
            string diStaQueryResult = string.Empty;            

            if (diStaQueryMessage == "00 00 00 00 00 06 01 01 00 00 00 04")
            {
                ushort diStaValue = 0x00;
                string diStaValStr = string.Empty;

                if (DIStateCh1 == EnumDIState.HighLevel)
                {
                    diStaValue |= 0x1;
                }
                else
                {
                    diStaValue &= 0xE;
                }

                if (DIStateCh2 == EnumDIState.HighLevel)
                {
                    diStaValue |= 0x2;
                }
                else
                {
                    diStaValue &= 0xD;
                }

                if (DIStateCh3 == EnumDIState.HighLevel)
                {
                    diStaValue |= 0x4;
                }
                else
                {
                    diStaValue &= 0xB;
                }

                if (DIStateCh4 == EnumDIState.HighLevel)
                {
                    diStaValue |= 0x8;
                }
                else
                {
                    diStaValue &= 0x7;
                }

                diStaValStr = diStaValue.ToString("X2");
                diStaQueryResult = string.Format("00 00 00 00 00 04 01 01 01 {0}", diStaValStr);
            }

            return diStaQueryResult;
        }

        public string GetDIStateChangeAutoNotifyMessage(int diChannel, EnumDIStateChange staChangeCase)
        {
            string message = string.Empty;

            switch(diChannel)
            {
                case 1:
                    message = staChangeCase == EnumDIStateChange.LowToHigh ? 
                              "00 00 00 00 00 08 00 05 00 10 ff 00" :
                              "00 00 00 00 00 08 00 05 00 10 00 00";
                    break;
                case 2:
                    message = staChangeCase == EnumDIStateChange.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 11 ff 00" :
                              "00 00 00 00 00 08 00 05 00 11 00 00";
                    break;
                case 3:
                    message = staChangeCase == EnumDIStateChange.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 12 ff 00" :
                              "00 00 00 00 00 08 00 05 00 12 00 00";
                    break;
                case 4:
                    message = staChangeCase == EnumDIStateChange.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 13 ff 00" :
                              "00 00 00 00 00 08 00 05 00 13 00 00";
                    break;
            }

            return message;
        }       
    }
}
