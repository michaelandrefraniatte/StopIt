using System;
using System.Collections.Generic;
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

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int processid = 0;
        private static List<string> procBLs = new List<string>(), procbls = new List<string>(), procblrecs = new List<string>(), processes = new List<string>();
        private static string procnames = "", procnamesbl = "";
        private static bool closed = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            TimeBeginPeriod(1);
            NtSetTimerResolution(1, true, ref CurrentResolution);
            using (StreamReader file = new StreamReader("siblacklist.txt"))
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
            Task.Run(() => StartStopItBlock());
        }
        public void StartStopItBlock()
        {
            listView1.View = View.Details;
            listView1.GridLines = true;
            listView1.MultiSelect = true;
            listView1.CheckBoxes = true;
            listView1.Columns.Add("Process");
            listView1.Columns[0].Width = 370;
            procnames = processnames();
            procnames = procnames.Remove(procnames.Length - 1);
            procnames = procnames.Substring(1);
            procnames = procnames.Replace(".exe", "");
            processes = procnames.Split(',').ToList();
            List<string> list = processes.Union(procBLs).ToList();
            list = list.Distinct().ToList();
            int i = 0;
            foreach (string process in list)
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
            this.listView1.ItemCheck += ListView1_ItemCheck;
            while (!closed)
            {
                procnames = processnames();
                procnames = procnames.Remove(procnames.Length - 1);
                procnames = procnames.Substring(1);
                procnames = procnames.Replace(".exe", "");
                processes = procnames.Split(',').ToList();
                procblrecs = procBLs;
                list = processes.Union(procblrecs).ToList();
                list = list.Distinct().ToList();
                foreach (string proc in list)
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
                Thread.Sleep(140);
            }
        }
        private void ListView1_ItemCheck(object sender, ItemCheckEventArgs e)
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
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            Thread.Sleep(200);
            procbls = procBLs.Distinct().ToList();
            using (StreamWriter createdfile = new StreamWriter("siblacklist.txt"))
            {
                foreach (string procbl in procbls)
                {
                    createdfile.WriteLine(procbl);
                }
                createdfile.WriteLine("");
                createdfile.WriteLine("");
                createdfile.Close();
            }
        }
    }
}