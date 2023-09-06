using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        [DllImport("stopitkills.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "get_cpu_usage")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string get_cpu_usage(int pid);
        [DllImport("stopitkills.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "get_memory_usage")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string get_memory_usage(int pid);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int processid = 0;
        private static List<string> procWLs = new List<string>(), procwls = new List<string>(), ProcWLRecs = new List<string>(), ProcWLSplitRecs = new List<string>(), procwlrecs = new List<string>(), processes = new List<string>();
        private static string procnames;
        private static bool closed = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            using (StreamReader file = new StreamReader("siwhitelist.txt"))
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
                        procWLs.Add(procName);
                        procnames += procName + ".exe ";
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
            listView1.Columns.Add("CPU");
            listView1.Columns.Add("Memory");
            listView1.Columns[0].Width = 240;
            listView1.Columns[1].Width = 60;
            listView1.Columns[2].Width = 100;
            procnames = processnames();
            procnames = procnames.Remove(procnames.Length - 1);
            procnames = procnames.Substring(1);
            procnames = procnames.Replace(".exe", "");
            processes = procnames.Split(',').ToList();
            List<string> list = processes.Union(procWLs).ToList();
            int i = 0;
            foreach (string process in list)
            {
                ListViewItem item = new ListViewItem();
                item.Text = process;
                item.SubItems.Add("0");
                item.SubItems.Add("0");
                listView1.Items.Add(item);
                foreach (string proc in procWLs)
                {
                    if (proc == process)
                    {
                        listView1.Items[i].Checked = true;
                        break;
                    }
                }
                i++;
            }
            this.listView1.ItemCheck += ListView1_ItemCheck1;
            Task.Run(() => StartStopItCpuMemory());
            while (!closed)
            {
                string procNames = procnames;
                ProcWLSplitRecs = killProcessByNames(procNames).Replace(".exe", "").Split(',').ToList();
                foreach (string ProcWLSplitRec in ProcWLSplitRecs)
                {
                    if (ProcWLSplitRec != "")
                    {
                        ProcWLRecs.Add(ProcWLSplitRec);
                        if (list.IndexOf(ProcWLSplitRec) < 0)
                        {
                            ListViewItem item = new ListViewItem();
                            item.Text = ProcWLSplitRec;
                            item.SubItems.Add("0");
                            item.SubItems.Add("0");
                            listView1.Items.Add(item);
                            list.Add(ProcWLSplitRec);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        private void ListView1_ItemCheck1(object sender, ItemCheckEventArgs e)
        {
            System.Windows.Forms.ListView listview = listView1;
            if (listview.Items[e.Index].Checked)
            {
                procWLs.Add(listview.Items[e.Index].Text);
                procnames += listview.Items[e.Index].Text + ".exe ";
            }
            else
            {
                int index = procWLs.IndexOf(listview.Items[e.Index].Text);
                procWLs.RemoveAt(index);
                procnames = "";
                foreach (string procName in procWLs)
                {
                    procnames += procName + ".exe ";
                }
            }
        }
        public void StartStopItCpuMemory()
        {
            while (!closed)
            {
                ListView listview = listView1;
                int i = 0;
                foreach (var item in listview.Items)
                {
                    string procname = listview.Items[i].Text; 
                    var processes = Process.GetProcessesByName(procname);
                    int cpu = 0;
                    int memory = 0;
                    foreach (var p in processes)
                    {
                        cpu += Convert.ToInt32(get_cpu_usage(p.Id));
                        memory += Convert.ToInt32(get_memory_usage(p.Id));
                    }
                    listView1.Items[i].SubItems[1].Text = cpu.ToString();
                    listView1.Items[i].SubItems[2].Text = memory.ToString();
                    i++;
                }
                Thread.Sleep(5000);
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            Thread.Sleep(200);
            procwlrecs = ProcWLRecs;
            using (StreamWriter createdfile = new StreamWriter("sirecord.txt"))
            {
                foreach (string procrec in procwlrecs)
                {
                    createdfile.WriteLine(procrec);
                }
                createdfile.Close();
            }
            procwls = procWLs;
            using (StreamWriter createdfile = new StreamWriter("siwhitelist.txt"))
            {
                foreach (string procwl in procwls)
                {
                    createdfile.WriteLine(procwl);
                }
                createdfile.WriteLine("");
                createdfile.Close();
            }
        }
    }
}
