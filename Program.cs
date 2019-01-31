using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;


namespace susp_resume_process
{
    class Program
    {
        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200)
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
        [DllImport("kernel32.dll")]
        static extern uint SuspendThread(IntPtr hThread);
        [DllImport("kernel32.dll")]
        static extern int ResumeThread(IntPtr hThread);
        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool CloseHandle(IntPtr handle);

        // to suspend a process//
        private static void SuspendProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                SuspendThread(pOpenThread);

                CloseHandle(pOpenThread);
            }
        }
        //resume a process suspended process//
        public static void ResumeProcess(int pid)
        {
            var process = Process.GetProcessById(pid);

            if (process.ProcessName == string.Empty)
                return;

            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                CloseHandle(pOpenThread);
            }
        }
        //to get process id by its name//
        static int get_process_id(string prname)
        {
            int prid=0;
            Process[] procList = Process.GetProcesses();
            for (int i = 0; i < procList.Length; i++)
                {
                    if (procList[i].ProcessName.ToString() == prname)
                    {
                        prid = procList[i].Id;
                        //procList[i].Kill();
                    }
                }

            MessageBox.Show(prid.ToString());
            return prid;
            
        }

        //to check weather the process is running or not//
        static bool is_process_running(string prname)
        {
            Process[] pname = Process.GetProcessesByName(prname);
            if (pname.Length == 0)
                return false;
            else
            {
                //MessageBox.Show("Process ID is:" + pname.Length);
                MessageBox.Show("The Process is currently running.", "Process Status", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }              
        }





        static void Main(string[] args)
        {
            string prname = "notepad";
            if (is_process_running(prname))
                ResumeProcess(get_process_id(prname));
            else
                MessageBox.Show("no process named '" + prname + "' found");
        }
    }
}
