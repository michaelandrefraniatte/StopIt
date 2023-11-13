using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int processid = 0;
        private static List<string> procBLs = new List<string>(), procbls = new List<string>(), procblrecs = new List<string>(), processes = new List<string>();
        private static List<string> servBLs = new List<string>(), servbls = new List<string>(), servblrecs = new List<string>(), services = new List<string>();
        private static string procnames = "", procnamesbl = "", servnames = "", servnamesbl = "";
        private static bool closed = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
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
            Task.Run(() => StartStopItBlock());
        }
        public void StartStopItBlock()
        {
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
                ListViewItem item = new ListViewItem();
                item.Text = process;
                listView1.Items.Add(item);
                if (procBLs.Contains(process))
                {
                    listView1.Items[i].Checked = true;
                }
                i++;
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
            }
            servlist = servlist.Union(servBLs).ToList();
            proclist = proclist.Distinct().ToList();
            int j = 0;
            foreach (ServiceController service in services)
            {
                if (service.Status == ServiceControllerStatus.Running | servBLs.Contains(service.ServiceName))
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = service.ServiceName;
                    listView2.Items.Add(item);
                    if (servBLs.Contains(service.ServiceName))
                    {
                        listView2.Items[j].Checked = true;
                    }
                    j++;
                }
            }
            this.listView1.ItemCheck += ListView1_ItemCheck;
            this.listView2.ItemCheck += ListView2_ItemCheck;
            while (!closed)
            {
                try
                {
                    procnames = processnames();
                    procnames = procnames.Remove(procnames.Length - 1);
                    procnames = procnames.Substring(1);
                    procnames = procnames.Replace(".exe", "");
                    processes = procnames.Split(',').ToList();
                    procblrecs = procBLs;
                    proclist = processes.Union(procblrecs).ToList();
                    proclist = proclist.Distinct().ToList();
                    foreach (string proc in proclist)
                    {
                        if (this.listView1.FindItemWithText(proc) == null & proc != "")
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = proc;
                            listView1.Items.Add(item);
                        }
                    }
                    string procNames = procnamesbl;
                    if (procNames != "")
                        procblrecs = killProcessByNames(procNames).Replace(".exe", "").Split(',').ToList();
                    services = ServiceController.GetServices();
                    servlist = new List<string>();
                    foreach (ServiceController service in services)
                    {
                        if (service.Status == ServiceControllerStatus.Running)
                        {
                            servlist.Add(service.ServiceName);
                        }
                    }
                    servlist = servlist.Union(servBLs).ToList();
                    proclist = proclist.Distinct().ToList();
                    foreach (string serv in proclist)
                    {
                        ServiceController service = new ServiceController(serv);
                        if (this.listView2.FindItemWithText(service.ServiceName) == null)
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = service.ServiceName;
                            listView2.Items.Add(item);
                        }
                        ListView listview = listView2;
                        var listviewitem = this.listView2.FindItemWithText(service.ServiceName);
                        if (listview.Items[listviewitem.Index].Checked)
                        {
                            int index = servBLs.IndexOf(listview.Items[listviewitem.Index].Text);
                            if (index >= 0 & service.Status == ServiceControllerStatus.Running)
                            {
                                service.Stop();
                                var timeout = new TimeSpan(0, 0, 5);
                                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                            }
                        }
                    }
                }
                catch { }
                Thread.Sleep(300);
            }
        }
        private void ListView1_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                ListView listview = listView1;
                procBLs = procBLs.Distinct().ToList();
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
                        }
                    }
                }
                else
                {
                    procBLs.Add(listview.Items[e.Index].Text);
                    procnamesbl = "";
                    foreach (string procName in procBLs)
                    {
                        procnamesbl += procName + ".exe ";
                    }
                }
            }
            catch { }
        }
        private void ListView2_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                ListView listview = listView2;
                servBLs = servBLs.Distinct().ToList();
                if (listview.Items[e.Index].Checked)
                {
                    int index = servBLs.IndexOf(listview.Items[e.Index].Text);
                    if (index >= 0)
                    {
                        servBLs.RemoveAt(index);
                        ServiceController service = new ServiceController(listview.Items[e.Index].Text);
                        if (service.Status == ServiceControllerStatus.Stopped)
                        {
                            service.Start();
                            var timeout = new TimeSpan(0, 0, 5);
                            service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                        }
                    }
                }
                else
                {
                    servBLs.Add(listview.Items[e.Index].Text);
                    ServiceController service = new ServiceController(listview.Items[e.Index].Text);
                    if (service.Status == ServiceControllerStatus.Running)
                    {
                        service.Stop();
                        var timeout = new TimeSpan(0, 0, 5);
                        service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                    }
                }
            }
            catch { }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            Thread.Sleep(200);
            procbls = procBLs.Distinct().ToList();
            using (StreamWriter createdfile = new StreamWriter("siprocblacklist.txt"))
            {
                foreach (string procbl in procbls)
                {
                    createdfile.WriteLine(procbl);
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
                }
                createdfile.WriteLine("");
                createdfile.WriteLine("");
                createdfile.Close();
            }
        }
    }
}