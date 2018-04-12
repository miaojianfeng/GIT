using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ETSL.Utilities
{
    public static class Auxiliaries
    {
        private const int SW_NORMAL = 1;

        // ----- Generate Time Stamp String -----
        public static string DateTimeInfoGenerate()
        {
            DateTime dt = DateTime.Now;
            string dt24 = dt.ToString("yyyy-MM-dd HH:mm:ss");
            string[] tmp = dt24.Split(new string[] { " " }, StringSplitOptions.None);
            string[] tmpDate = tmp[0].Split(new string[] { "-" }, StringSplitOptions.None);
            string[] tmpTime = tmp[1].Split(new string[] { ":" }, StringSplitOptions.None);

            string date = string.Format("{0}-{1}-{2}", tmpDate[0], tmpDate[1], tmpDate[2]);
            string time = string.Format("{0}-{1}-{2}-{3}", tmpTime[0], tmpTime[1], tmpTime[2], dt.Millisecond.ToString());
            string timeStamp = string.Format("{0} [{1}]", time, date);
            return timeStamp;            
        }

        public static string TimeStampGenerate()
        {
            DateTime dt = DateTime.Now;
            string dt24 = dt.ToString("yyyy-MM-dd HH:mm:ss fff");
            string[] tmp = dt24.Split(new string[] { " " }, StringSplitOptions.None);
            return string.Format("{0} {1}", tmp[1], tmp[2]);
        }

        public static string GetFileNameFromFullPath(string fullPath)
        {
            string fileName = string.Empty;
            string[] array = fullPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            fileName = array[array.Length - 1];
            return fileName;
        }

        public static string GetFolderNameFromFullFilePath(string fileFullPath)
        {
            StringBuilder folderName = new StringBuilder(30);
            string[] array = fileFullPath.Split(new string[] { "\\" }, StringSplitOptions.None);
            int count = array.Length - 1;
            for(int i=0; i<count;i++)
            {
                folderName.Append(array[i]);
            }
            return folderName.ToString();
        }

        public static void LaunchProgram(string exeName, string processName, string folder)
        {
            bool simRunning = false;
            Process[] processList = Process.GetProcesses();
            foreach (Process process in processList)
            {
                if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    simRunning = true;
                    ShowWindow(process.MainWindowHandle, SW_NORMAL);
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }

            if (!simRunning)
            {
                System.Diagnostics.Process exep = System.Diagnostics.Process.Start(folder + "\\" + exeName);
            }
        }

        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        private static extern int ShowWindow(System.IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL", CharSet = CharSet.Auto)]
        private static extern bool SetForegroundWindow(System.IntPtr hWnd);
    }
}
