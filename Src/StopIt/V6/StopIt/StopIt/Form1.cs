using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ListView = System.Windows.Forms.ListView;

namespace StopIt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("stopitkills.dll", EntryPoint = "processnames")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string processnames();
        [DllImport("stopitkills.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "killProcessByNames")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string killProcessByNames(string processnames);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int processid = 0;
        private static List<string> procBLs = new List<string>(), procbls = new List<string>(), processes = new List<string>();
        private static List<string> servBLs = new List<string>(), servbls = new List<string>(), servblrecs = new List<string>();
        private static string procnames = "", procnamesbl = "", procNames = "", servNames = "";
        private static TimeSpan timeout = new TimeSpan(0, 0, 1);
        private static ListViewItem itemproc, itemserv;
        private static Process[] edgeprocesses;
        private static bool edgechecking = false;
        private static bool closed = false, closeonicon = false;
        private void Form1_Shown(object sender, EventArgs e)
        {
            using (StreamWriter createdfile = new StreamWriter(Application.StartupPath + @"\temphandle"))
            {
                createdfile.WriteLine(Process.GetCurrentProcess().MainWindowHandle);
            }
            TrayMenuContext();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            Task.Run(() => StartStopItBlockProc());
            Task.Run(() => StartStopItBlockServ());
            Task.Run(() => StartStopItEdge());
        }
        public void StartStopItBlockProc()
        {
            using (StreamReader file = new StreamReader("siprocblacklist.txt"))
            {
                while (!closed)
                {
                    string procName = file.ReadLine();
                    if (procName == "")
                    {
                        file.Close();
                        break;
                    }
                    else
                    {
                        procBLs.Add(procName);
                        procnamesbl += procName + ".exe ";
                    }
                }
            }
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.MultiSelect = true;
            listView1.CheckBoxes = true;
            listView1.Columns.Add("Process");
            listView1.Columns[0].Width = 175;
            procnames = processnames();
            procnames = procnames.Remove(procnames.Length - 1);
            procnames = procnames.Substring(1);
            procnames = procnames.Replace(".exe", "");
            processes = procnames.Split(',').ToList();
            List<string> proclist = processes.Union(procBLs).ToList();
            proclist = proclist.Distinct().ToList();
            int i = 0;
            foreach (string process in proclist)
            {
                itemproc = new ListViewItem();
                itemproc.Text = process;
                listView1.Items.Add(itemproc);
                if (procBLs.Contains(process))
                {
                    listView1.Items[i].Checked = true;
                }
                i++;
                Thread.Sleep(1);
            }
            this.listView1.ItemCheck += ListView1_ItemCheck;
            for (; ; )
            {
                if (closed)
                    break;
                procnames = processnames();
                procnames = procnames.Remove(procnames.Length - 1);
                procnames = procnames.Substring(1);
                procnames = procnames.Replace(".exe", "");
                processes = procnames.Split(',').ToList();
                foreach (string proc in processes)
                {
                    if (this.listView1.FindItemWithText(proc) == null & proc != "")
                    {
                        itemproc = new ListViewItem();
                        itemproc.Text = proc;
                        listView1.Items.Add(itemproc);
                    }
                    Thread.Sleep(1);
                }
                procNames = procnamesbl;
                if (procNames != "")
                    killProcessByNames(procNames);
                Thread.Sleep(1000);
            }
        }
        public void StartStopItBlockServ()
        {
            using (StreamReader file = new StreamReader("siservblacklist.txt"))
            {
                while (!closed)
                {
                    string servName = file.ReadLine();
                    if (servName == "")
                    {
                        file.Close();
                        break;
                    }
                    else
                    {
                        servBLs.Add(servName);
                    }
                }
            }
            listView2.View = View.Details;
            listView2.GridLines = true;
            listView2.MultiSelect = true;
            listView2.CheckBoxes = true;
            listView2.Columns.Add("Service");
            listView2.Columns[0].Width = 175;
            ServiceController[] services = ServiceController.GetServices();
            List<string> servlist = new List<string>();
            foreach (ServiceController service in services)
            {
                if (service.Status == ServiceControllerStatus.Running)
                {
                    servlist.Add(service.ServiceName);
                }
                Thread.Sleep(1);
            }
            servlist = servlist.Union(servBLs).ToList();
            servlist = servlist.Distinct().ToList();
            int j = 0;
            foreach (string serv in servlist)
            {
                itemserv = new ListViewItem();
                itemserv.Text = serv;
                listView2.Items.Add(itemserv);
                if (servBLs.Contains(serv))
                {
                    listView2.Items[j].Checked = true;
                }
                j++;
                Thread.Sleep(1);
            }
            this.listView2.ItemCheck += ListView2_ItemCheck;
            for (; ; )
            {
                if (closed)
                    break;
                services = ServiceController.GetServices();
                foreach (ServiceController service in services)
                {
                    try
                    {
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            if (this.listView2.FindItemWithText(service.ServiceName) == null)
                            {
                                itemserv = new ListViewItem();
                                itemserv.Text = service.ServiceName;
                                listView2.Items.Add(itemserv);
                            }
                            servNames = service.ServiceName;
                            if (servNames.Length > 7)
                                servNames = servNames.Substring(0, 7);
                            servblrecs = servBLs;
                            if (servblrecs.Any(n => n.StartsWith(servNames)))
                            {
                                service.Stop();
                                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            }
                        }
                    }
                    catch { }
                    Thread.Sleep(1);
                }
                Thread.Sleep(1000);
            }
        }
        private void ListView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ListView listview = listView1;
            if (listview.Items[e.Index].Checked)
            {
                int index = procBLs.IndexOf(listview.Items[e.Index].Text);
                if (index >= 0)
                {
                    procBLs.RemoveAt(index);
                    procnamesbl = "";
                    foreach (string procName in procBLs)
                    {
                        procnamesbl += procName + ".exe ";
                        Thread.Sleep(1);
                    }
                }
            }
            else
            {
                int index = procBLs.IndexOf(listview.Items[e.Index].Text);
                if (index < 0)
                {
                    procBLs.Add(listview.Items[e.Index].Text);
                    procnamesbl = "";
                    foreach (string procName in procBLs)
                    {
                        procnamesbl += procName + ".exe ";
                        Thread.Sleep(1);
                    }
                }
            }
        }
        private void ListView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            ListView listview = listView2;
            if (listview.Items[e.Index].Checked)
            {
                int index = servBLs.IndexOf(listview.Items[e.Index].Text);
                if (index >= 0)
                {
                    servBLs.RemoveAt(index);
                }
            }
            else
            {
                int index = servBLs.IndexOf(listview.Items[e.Index].Text);
                if (index < 0)
                {
                    servBLs.Add(listview.Items[e.Index].Text);
                }
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            FillList();
        }
        [STAThread]
        private void FillList()
        {
            Thread newThread = new Thread(new ThreadStart(fillLists));
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.Start();
        }
        private void fillLists()
        {
            Thread.Sleep(3000);
            procbls = procBLs.Distinct().ToList();
            using (StreamWriter createdfile = new StreamWriter("siprocblacklist.txt"))
            {
                foreach (string procbl in procbls)
                {
                    createdfile.WriteLine(procbl);
                    Thread.Sleep(1);
                }
                createdfile.WriteLine("");
                createdfile.WriteLine("");
                createdfile.Close();
            }
            servbls = servBLs.Distinct().ToList();
            using (StreamWriter createdfile = new StreamWriter("siservblacklist.txt"))
            {
                foreach (string servbl in servbls)
                {
                    createdfile.WriteLine(servbl);
                    Thread.Sleep(1);
                }
                createdfile.WriteLine("");
                createdfile.WriteLine("");
                createdfile.Close();
            }
        }
        public void StartStopItEdge()
        {
            for (; ; )
            {
                if (closed)
                    break;
                try
                {
                    edgechecking = false;
                    edgeprocesses = Process.GetProcessesByName("msedge");
                    foreach (Process edgeprocess in edgeprocesses)
                    {
                        if (edgeprocess.MainWindowTitle.Length > 0)
                        {
                            edgechecking = true;
                            break;
                        }
                        Thread.Sleep(1);
                    }
                    if (!edgechecking & edgeprocesses.Length >= 5)
                    {
                        foreach (Process edgeprocess in edgeprocesses)
                        {
                            edgeprocess.Kill();
                            Thread.Sleep(1);
                        }
                    }
                }
                catch { }
                Thread.Sleep(1000);
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!closeonicon)
            {
                e.Cancel = true;
                MinimzedTray();
                return;
            }
        }
        private void TrayMenuContext()
        {
            this.notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon1.ContextMenuStrip.Items.Add("Quit", null, this.MenuTest1_Click);
        }
        void MenuTest1_Click(object sender, EventArgs e)
        {
            closeonicon = true;
            this.Close();
        }
        private void MinimzedTray()
        {
            ShowWindow(Process.GetCurrentProcess().MainWindowHandle, 0);
        }
        private void MaxmizedFromTray()
        {
            if (File.Exists(Application.StartupPath + @"\temphandle"))
                using (StreamReader file = new StreamReader(Application.StartupPath + @"\temphandle"))
                {
                    IntPtr handle = new IntPtr(int.Parse(file.ReadLine()));
                    ShowWindow(handle, 9);
                    SetForegroundWindow(handle);
                    Microsoft.VisualBasic.Interaction.AppActivate("StopIt");
                }
        }
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Task.Run(() => MaxmizedFromTray());
        }
    }
}