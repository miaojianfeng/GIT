using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            string result = ProcessDIStateQueryMessage(1, 0, 0, 1);
        }

        static public string ProcessDIStateQueryMessage(int DIStateCh1, int DIStateCh2, int DIStateCh3, int DIStateCh4)
        { 
                string diStaQueryResult = string.Empty;
                ushort diStaValue = 0x00;

                if (DIStateCh1 == 1)
                {
                    diStaValue |= 0x1;
                }
                else
                {
                    diStaValue &= 0xE;
                }

                if (DIStateCh2 == 1)
                {
                    diStaValue |= 0x2;
                }
                else
                {
                    diStaValue &= 0xD;
                }

                if (DIStateCh3 == 1)
                {
                    diStaValue |= 0x4;
                }
                else
                {
                    diStaValue &= 0xB;
                }

                if (DIStateCh4 == 1)
                {
                    diStaValue |= 0x8;
                }
                else
                {
                    diStaValue &= 0x7;
                }

            return diStaValue.ToString("X2");
        }
    }
}
