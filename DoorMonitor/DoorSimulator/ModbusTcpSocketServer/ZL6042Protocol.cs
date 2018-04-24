using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETSL.TcpSocket
{
    //public enum EnumDIReportMode
    //{
    //    Polling = 0,
    //    AutoNotify = 1
    //}

    public enum EnumDIState
    {
        LowLevel = 0,
        HighLevel = 1
    }

    public enum EnumDIStateChangeCase
    {
        LowToHigh,
        HighToLow
    }

    public class ZL6042DISimulator
    {
        // Constructor
        public ZL6042DISimulator()
        {
            DIStateCh1 = EnumDIState.HighLevel;
            DIStateCh2 = EnumDIState.HighLevel;
            DIStateCh3 = EnumDIState.HighLevel;
            DIStateCh4 = EnumDIState.HighLevel;
        }

        // Field

        // Property
        //public EnumDIReportMode DIReportMode { set; get; }

        public EnumDIState DIStateCh1 { private get; set; }
        public EnumDIState DIStateCh2 { private get; set; }
        public EnumDIState DIStateCh3 { private get; set; }
        public EnumDIState DIStateCh4 { private get; set; }

        // Method
        public string ProcessDIStateQueryMessage(string diStaQueryMessage)
        {
            string diStaQueryResult = string.Empty;            

            if (diStaQueryMessage == "00 00 00 00 00 06 01 01 00 10 00 04")
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
                diStaQueryResult = string.Format("00 00 00 00 00 06 01 01 01 {0} 11 8c", diStaValStr);
            }

            return diStaQueryResult;
        }

        public string GetDIStateChangeAutoNotifyMessage(int diChannel, EnumDIStateChangeCase staChangeCase)
        {
            string message = string.Empty;

            switch(diChannel)
            {
                case 1:
                    message = staChangeCase == EnumDIStateChangeCase.LowToHigh ? 
                              "00 00 00 00 00 08 00 05 00 10 ff 00 8C 2E" :
                              "00 00 00 00 00 08 00 05 00 10 00 00 CD 2E";
                    break;
                case 2:
                    message = staChangeCase == EnumDIStateChangeCase.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 11 ff 00 DD EE" :
                              "00 00 00 00 00 08 00 05 00 11 00 00 9C 1E";
                    break;
                case 3:
                    message = staChangeCase == EnumDIStateChangeCase.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 12 ff 00 2D EE" :
                              "00 00 00 00 00 08 00 05 00 12 00 00 6C 1E";
                    break;
                case 4:
                    message = staChangeCase == EnumDIStateChangeCase.LowToHigh ?
                              "00 00 00 00 00 08 00 05 00 13 ff 00 7C 2E" :
                              "00 00 00 00 00 08 00 05 00 13 00 00 3D DE";
                    break;
            }

            return message;
        }       
    }
}
