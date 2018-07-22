using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NationalInstruments.VisaNS;

namespace ETSL.InstrDriver.Base
{
    public class InstrumentManager:INotifyPropertyChanged
    {
        // ---------- Constructor----------
        public InstrumentManager()
        {
            InstrumentDriverList = new ObservableCollection<VisaInstrDriver>();
            InstrNameList = new ObservableCollection<string>();
            VisaAddressList = new ObservableCollection<string>();
            VisaSearchMessage = string.Empty;
            VisaSearchException = string.Empty;
            this.currInstrDrv = null;
        }

        // ---------- Field --------------        
        private string visaSearchMessage = string.Empty;
        private string visaSearchException = string.Empty;
        private VisaInstrDriver currInstrDrv = null;        

        // ---------- Property ----------
        public ObservableCollection<string> InstrNameList { private set; get; }   
        public ObservableCollection<VisaInstrDriver> InstrumentDriverList { private set; get; }
        public ObservableCollection<string> VisaAddressList { private set; get; }      
        
        public VisaInstrDriver CurrentInstrumentDriver
        {
            get
            {
                return this.currInstrDrv;
            }
            set
            {
                if(InstrumentDriverList.Contains(value))
                {
                    this.currInstrDrv = value;
                    NotifyPropertyChanged("CurrentInstrumentDriver");
                }
            }
        } 
        
        public string VisaSearchMessage
        {
            private set
            {
                this.visaSearchMessage = value;
                NotifyPropertyChanged("VisaSearchMessage");
            }

            get
            {
                return visaSearchMessage;                
            }
        } 
        
        public string VisaSearchException
        {
            private set
            {
                this.visaSearchException = value;
                NotifyPropertyChanged("VisaSearchException");
            }
            
            get
            {
                return this.visaSearchException;
            }   
        }      

        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------  
        public void AddInstrument(VisaInstrDriver driver)
        {
            InstrumentDriverList.Add(driver);
            InstrNameList.Add(driver.InstrumentName);
        }

        public bool FindVisaResources()
        {
            bool retValue = false;

            try
            {
                VisaSearchMessage = "Searching VISA Resources, please wait...";
                string[] resources = ResourceManager.GetLocalManager().FindResources("?*");

                if (resources.Length == 0)
                {
                    VisaSearchMessage = "No VISA resources founded!";
                    retValue = false;
                }
                else
                {
                    VisaSearchMessage = "VISA resources founded!";
                    retValue = true;

                    VisaAddressList.Clear();
                    foreach (string s in resources)
                    {
                        HardwareInterfaceType intType;
                        short intNum;
                        ResourceManager.GetLocalManager().ParseResource(s, out intType, out intNum);
                        VisaAddressList.Add(s);
                    }
                }
            }
            catch (VisaException ex)
            {
                VisaSearchMessage = "Exception!";
                VisaSearchException = "VISA Exception: " + ex.Message;
                retValue = false;
            }
            catch (Exception ex)
            {
                VisaSearchMessage = "Exception!";
                VisaSearchException = "Exception: " + ex.Message;
                retValue = false;
            }

            return retValue;
        }

        public async Task<bool> FindVisaResourcesAsync()
        {
            Task<bool> task = new Task<bool>(FindVisaResources);
            task.Start();
            await task;
            return task.Result;
        }

        // ---------- Private Method ----------
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }    
}
