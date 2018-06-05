using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NationalInstruments.VisaNS;

namespace ETSL.InstrDriver.Base
{
    public class InstrDriverException : Exception
    {
        #pragma warning disable 1591
        public InstrDriverException() { }

        public InstrDriverException(string message) : base(message) { }

        public InstrDriverException(string message, Exception inner) : base(message, inner) { }

        protected InstrDriverException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
        #pragma warning restore 1591
    }
    
    public class VisaInstrDriver: INotifyPropertyChanged
    {
        // ---------- Constructor ----------
        public VisaInstrDriver()
        {
            this.visaAddress = string.Empty;
            this.hasInitialized = false;
            this.execFailed = false;
            this.lastError.Clear();
            this.queueError.Clear();
            EnableErrorLogging = false;
        }

        public VisaInstrDriver(string visaAddr)
        {
            VisaAddress = visaAddr;
            if (string.IsNullOrEmpty(visaAddress))
                throw new InstrDriverException("VISA address is empty!");

            this.visaAddress = visaAddr;
            this.hasInitialized = false;
            this.execFailed = false;
            this.lastError.Clear();
            this.queueError.Clear();
            EnableErrorLogging = false;
        }

        // ---------- Field ----------
        private MessageBasedSession mbSession;
        private string visaAddress = string.Empty;
        private string instrID = string.Empty;
        private bool hasInitialized = false;
        private bool execFailed = false;  
        private StringBuilder lastError = new StringBuilder(100);
        private Queue<string> queueError = new Queue<string>(200);
        private Stopwatch stopWatch = new Stopwatch();

        // ---------- Property ----------
        public string VisaAddress
        {
            get
            {
                return this.visaAddress;
            }

            set
            {
                this.visaAddress = value;
                NotifyPropertyChanged("VisaAddress");
            }
        }

        public string InstrumentName { protected set; get; }

        public bool HasInitialized
        {
            get
            {
                return this.hasInitialized;
            }
            protected set
            {
                this.hasInitialized = value;
                NotifyPropertyChanged("HasInitialized");
            }            
        }

        public string InstrID
        {
            get
            {
                return this.instrID;
            }
            protected set
            {
                this.instrID = value;
                NotifyPropertyChanged("InstrID");
            }
        }

        public bool ExecFailed
        {
            get
            {
                return this.execFailed;
            }

            protected set
            {
                this.execFailed = value;
                NotifyPropertyChanged("ExecFailed");
            }
        }

        public string LastError
        {
            get
            {
                return this.lastError.ToString();
            }

            protected set
            {
                this.lastError.Clear();

                if (!string.IsNullOrEmpty(value))
                {                    
                    this.lastError.Append(value);
                } 

                NotifyPropertyChanged("LastError");
            }
        }

        public bool EnableErrorLogging { set; get; }

        // ---------- Event ----------
        public event PropertyChangedEventHandler PropertyChanged;

        // ---------- Method ----------
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string TrimStringEnd(string textRaw)
        {
            string text = string.Empty;
            if (textRaw.EndsWith("\\n") || textRaw.EndsWith("\n"))
            {
                string[] tmp = textRaw.Split(new string[] { "\n", "\\n" }, StringSplitOptions.None);
                text = tmp[0];
            }
            else
            {
                text = textRaw;
            }

            return text;
        }

        //private string ReplaceCommonEscapeSequences(string s)
        //{
        //    return s.Replace("\\n", "\n").Replace("\\r", "\r");
        //}

        //private string InsertCommonEscapeSequences(string s)
        //{
        //    return s.Replace("\n", "\\n").Replace("\r", "\\r");
        //}

        protected void ErrorHandling(string errInfo)
        {
            ExecFailed = true;
            LastError = errInfo;

            if (EnableErrorLogging)
            {
                queueError.Enqueue(errInfo + "\n");
            }
        }

        protected bool Open()
        {
            try
            {
                Close();

                this.mbSession = (MessageBasedSession)ResourceManager.GetLocalManager().Open(VisaAddress, AccessModes.NoLock, 10000);
                this.mbSession.SetAttributeBoolean(AttributeType.TermcharEn, true);
                this.mbSession.SetAttributeInt16(AttributeType.Termchar, 10);
                this.mbSession.SetAttributeInt32(AttributeType.TmoValue, 10000);

                return true;
            }
            catch (Exception ex)
            {
                ErrorHandling(string.Format("Open VISA session <{0}> failed! ==> Exception: {1}", VisaAddress, ex.Message));
                return false;
            }            
        }

        protected void Close()
        {
            if (this.mbSession != null)
            { 
                this.mbSession.Dispose();
                LastError = string.Empty;
                ExecFailed = false;
                InstrID = string.Empty;
                HasInitialized = false;
            }
        }

        public void SendCommand(string command)
        {
            try
            {
                mbSession.Write(command+"\n");
                //System.Threading.Thread.Sleep(50);
                ExecFailed = false;
            }
            catch (Exception ex)
            {
                ErrorHandling(string.Format("Sending command <{0}> failed! ==> Exception: {1}", command, ex.Message));
                //throw new InstrDriverException(LastError);  
                MessageBox.Show(LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);      
            }
        }

        public string ReadCommand()
        {
            string resp = string.Empty;

            try
            { 
                resp = mbSession.ReadString();
                ExecFailed = false;
            }
            catch (Exception ex)
            {
                resp = string.Empty;
                ErrorHandling(string.Format("Reading command response failed! ==> Exception: {0}", ex.Message));
                //throw new InstrDriverException(LastError);
                MessageBox.Show(LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return TrimStringEnd(resp);
        }

        public string QueryCommand(string command)
        {
            string resp = string.Empty;

            try
            { 
                resp = mbSession.Query(command);
                ExecFailed = false;
            }
            catch (Exception ex)
            {
                resp = string.Empty;
                ErrorHandling(string.Format("Querying command <{0}> failed! ==> Exception: {1}", command, ex.Message));
                //throw new InstrDriverException(LastError);  
                MessageBox.Show(LastError, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            return TrimStringEnd(resp);
        }

        public virtual bool Initialize()
        {
            if (Open())
            {
                HasInitialized = true;
                ExecFailed = false;
                return true;
            }
            else
            {                
                ErrorHandling(string.Format("VisaInstrDriver.Initialize Failed! ==> Exception: {0}", LastError));
                return false;
            }
        }

        public virtual void DeInitialize()
        {
            Close();            
        }

        protected bool WaitOperationComplete(int queryInterval_ms, int timeout_ms)
        {
            bool retValue = false;

            while (true)
            {
                this.stopWatch.Start();
                System.Threading.Thread.Sleep(queryInterval_ms);
                string result = QueryCommand("*OPC?");

                if (result == "+1" || result == "1" || result =="1." || result =="+1.")
                {
                    retValue = true;
                    break;
                }

                if (this.stopWatch.ElapsedMilliseconds == timeout_ms)
                {
                    retValue = false;
                    break;
                }
            }
            return retValue;
        }
    }
}
