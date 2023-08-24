using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetFwTypeLib;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace StopIt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private Type program;
        private object obj;
        private Assembly assembly;
        private System.CodeDom.Compiler.CompilerResults results;
        private Microsoft.CSharp.CSharpCodeProvider provider;
        private System.CodeDom.Compiler.CompilerParameters parameters;
        private string code = @"
                using System;
                using System.Runtime.InteropServices;
                using System.Threading.Tasks;
                using System.Threading;
                using System.Diagnostics;
                using System.Collections.Generic;
                using System.Linq;
                using System.Windows.Forms;
                using System.Collections;
                using System.Security.Principal;
                namespace StringToCode
                {
                    public class FooClass 
                    { 
                        [DllImport(""winmm.dll"", EntryPoint = ""timeBeginPeriod"")]
                        private static extern uint TimeBeginPeriod(uint ms);
                        [DllImport(""winmm.dll"", EntryPoint = ""timeEndPeriod"")]
                        private static extern uint TimeEndPeriod(uint ms);
                        [DllImport(""ntdll.dll"", EntryPoint = ""NtSetTimerResolution"")]
                        private static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
                        [DllImport(""kernel32.dll"")]
                        private static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Int32 bInheritHandle, UInt32 dwProcessId);
                        [DllImport(""kernel32.dll"")]
                        private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt64 size, out IntPtr lpNumberOfBytesRead);
                        [DllImport(""kernel32.dll"")]
                        private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt64 size, out IntPtr lpNumberOfBytesRead);
                        [DllImport(""kernel32.dll"")]
                        private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr size, uint flAllocationType, uint lpflOldProtect);
                        [DllImport(""kernel32.dll"")]
                        private static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, IntPtr size, uint flNewProtect, out uint lpflOldProtect);
                        [DllImport(""kernel32.dll"")]
                        private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out uint basicInformation, IntPtr dwSize);
                        [DllImport(""kernel32.dll"", SetLastError = true, ExactSpelling = true)]
                        private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint dwFreeType);
                        [DllImport(""kernel32.dll"")]
                        private static extern bool VirtualLock(IntPtr lpAddress, IntPtr dwSize);
                        [DllImport(""kernel32.dll"")]
                        private static extern bool VirtualUnlock(IntPtr lpAddress, IntPtr dwSize);
                        [DllImport(""kernel32.dll"")]
                        public static extern bool SetProcessWorkingSetSizeEx(IntPtr hProcess, int dwMinimumWorkingSetSize, int dwMaximumWorkingSetSize, int Flags);
                        const Int32 SW_MINIMIZE = 6;
                        public static ThreadStart threadstart;
                        public static Thread thread;
                        public static uint CurrentResolution = 0;
                        public static int processid = 0;
                        private static List<string> procnames = new List<string>();
                        private static string datasetit, datauseit, dataputit;
                        private static bool closed = false;
                        private static Action action;
                        private static Task task;
                        private static PerformanceCounter counter;
                        private static uint dwdesiredaccess = PROCESS_ALL_ACCESS;
                        private static IntPtr pP;
                        public static uint oP, lpOldProtect;
                        public static Int64 bA;
                        public static Int64 lA;
                        public static IntPtr A, Ai, sizei;
                        public static UInt64 sizeu;
                        private static int effic;
                        public static int size;
                        public static byte[] fbytes;
                        private static IntPtr lpNumberOfBytesRead;
                        private static Random rnd = new Random();
                        private static List<byte> list;
                        const uint DELETE = 0x00010000;
                        const uint READ_CONTROL = 0x00020000;
                        const uint WRITE_DAC = 0x00040000;
                        const uint WRITE_OWNER = 0x00080000;
                        const uint SYNCHRONIZE = 0x00100000;
                        const uint END = 0xFFF;
                        const uint PROCESS_ALL_ACCESS = DELETE | READ_CONTROL | WRITE_DAC | WRITE_OWNER | SYNCHRONIZE | END;
                        const uint PROCESS_VM_OPERATION = 0x0008;
                        const uint PROCESS_VM_READ = 0x0010;
                        const uint PROCESS_VM_WRITE = 0x0020;
                        const uint PROCESS_CREATE_THREAD = 0x0002;
                        const uint PROCESS_QUERY_INFORMATION = 0x0400;
                        const uint PAGE_READWRITE = 0x04;
                        const uint MEM_COMMIT = 0x00001000;
                        const uint PAGE_EXECUTE_READWRITE = 0x40;
                        const uint MEM_DECOMMIT = 0x00004000;
                        const uint MEM_RESERVE = 0x00002000;
                        const uint PAGE_READONLY = 0x02;
                        const uint PAGE_GUARD = 0x100;
                        public static void Main()
                        {
                            TimeBeginPeriod(1);
                            NtSetTimerResolution(1, true, ref CurrentResolution);
                            SetProcessPriority();
                            Task.Run(() => Start());
                        }
                        private static void SetProcessPriority()
                        {
                            using (Process p = Process.GetCurrentProcess())
                            {
                                p.PriorityClass = ProcessPriorityClass.RealTime;
                            }
                        }
                        private static void Start()
                        {
                            Task.Run(() => StartFlood());
                            System.Threading.Thread.Sleep(60000);
                            counter = new PerformanceCounter(""Process"", ""% Processor Time"", Process.GetCurrentProcess().ProcessName, true);
                            Task.Run(() => StartTick());
                        }
                        public static void StartFlood()
                        {
                            Put();
                            try
                            {
                                while (processid == 0)
                                {
                                    Process[] processes = Process.GetProcessesByName(""BlackOpsColdWar"");
                                    foreach (var p in processes)
                                    {
                                        processid = p.Id;
                                        break;
                                    }
                                    processes = Process.GetProcessesByName(""ModernWarfare"");
                                    foreach (var p in processes)
                                    {
                                        processid = p.Id;
                                        break;
                                    }
                                    processes = Process.GetProcessesByName(""Vanguard"");
                                    foreach (var p in processes)
                                    {
                                        processid = p.Id;
                                        break;
                                    }
                                    processes = Process.GetProcessesByName(""cod"");
                                    foreach (var p in processes)
                                    {
                                        processid = p.Id;
                                        break;
                                    }
                                    Thread.Sleep(1);
                                }
                                datasetit = setIt(processid);
                                task = Task.Run(action = () => Flood());
                            }
                            catch { }
                        }
                        private static void Flood()
                        {
                            while (!closed)
                            {
                                try
                                {
                                    datauseit = useIt();
                                }
                                catch { }
                                Thread.Sleep(1);
                            }
                        }
                        private static void Put()
                        {
                            try
                            {
                                dataputit = putIt();
                            }
                            catch  { }
                        }
                        private static void StartTick()
                        {
                            while (true)
                            {
                                try
                                {
                                    if (counter.NextValue() / Environment.ProcessorCount <= 0.50000)
                                    {
                                        closed = true;
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                    if (task.IsCompleted)
                                    {
                                        closed = false;
                                        Task.Run(() => StartFlood());
                                    }
                                }
                                catch { }
                                System.Threading.Thread.Sleep(60000);
                            }
                        }
                        public static string setIt(int processid)
                        {
                            try
                            {
                                TimeBeginPeriod(1);
                                NtSetTimerResolution(1, true, ref CurrentResolution);
                                pP = OpenProcess(dwdesiredaccess, 1, (uint)processid);
                                bA = 140000000;
                                lA = 8587934592;
                            }
                            catch (Exception e)
                            {
                                return e.ToString();
                            }
                            return """";
                        }
                        public static string useIt()
                        {
                            try
                            {
                                list = new List<byte>(fbytes);
                                Thread.Sleep(10);
                                list.RemoveAt(rnd.Next(0, list.Count));
                                Thread.Sleep(10);
                                list.Add((byte)rnd.Next(0, 256));
                                Thread.Sleep(10);
                                fbytes = list.ToArray();
                                Thread.Sleep(10);
                                for (int i = (int)bA; i < (int)lA; i = i + size)
                                {
                                    Ai = (IntPtr)i;
                                    A = VirtualAllocEx(pP, Ai, sizei, MEM_RESERVE | MEM_COMMIT, PAGE_READONLY | PAGE_GUARD);
                                    VirtualQueryEx(pP, A, out oP, sizei);
                                    VirtualProtectEx(pP, A, sizei, PAGE_GUARD | oP, out lpOldProtect);
                                    WriteProcessMemory(pP, A, fbytes, sizeu, out lpNumberOfBytesRead);
                                    VirtualQueryEx(pP, Ai, out oP, sizei);
                                    VirtualProtectEx(pP, Ai, sizei, PAGE_GUARD | oP, out lpOldProtect);
                                    WriteProcessMemory(pP, Ai, fbytes, sizeu, out lpNumberOfBytesRead);
                                }
                            }
                            catch (Exception e)
                            {
                                return e.ToString();
                            }
                            return """";
                        }
                        public static string putIt()
                        {
                            try
                            {
                                effic = rnd.Next(200, 300);
                                size = 2147483647 / effic;
                                fbytes = new byte[size];
                                sizei = (IntPtr)size;
                                sizeu = (UInt64)size;
                                for (int j = 0; j < size; j++)
                                {
                                    fbytes[j] = (byte)rnd.Next(0, 256);
                                }
                                return ""filled"";
                            }
                            catch (Exception e)
                            {
                                return e.ToString();
                            }
                        }
                        public static void Close()
                        {
                            try
                            {
                                closed = true;
                                Thread.Sleep(100);
                            }
                            catch { }
                        }
                    }
                }";
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
        private static List<string> ProcWLRecs = new List<string>(), ProcWLSplitRecs = new List<string>(), procwlrecs = new List<string>();
        private static string procnames;
        private static List<string> dnsnames = new List<string>(), DNSBLRecs = new List<string>(), dnsblrecs = new List<string>();
        private static List<string> scaledips = new List<string>();
        private static string hostname = "";
        private static INetFwRule2 newRule;
        private static INetFwPolicy2 firewallpolicy;
        private static string RemoteAdrr = "0.0.0.0", Scaledip = "0.0.0.0";
        private static IPAddress Addr;
        private static bool checking, checkingname;
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
                        procnames += procName + ".exe ";
                }
            }
            Task.Run(() => StartStopItWL());
            using (StreamReader file = new StreamReader("siwhitelistdns.txt"))
            {
                while (true)
                {
                    string dnsname = file.ReadLine();
                    if (dnsname == "")
                    {
                        file.Close();
                        break;
                    }
                    else
                        dnsnames.Add(dnsname);
                }
            }
            Task.Run(() => StartStopItDNS());
            parameters = new System.CodeDom.Compiler.CompilerParameters();
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = false;
            parameters.IncludeDebugInformation = false;
            parameters.CompilerOptions = "/optimize";
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Windows.Forms.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Drawing.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Runtime.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Collections.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Linq.dll");
            parameters.ReferencedAssemblies.Add(Application.StartupPath + @"\System.Security.dll");
            provider = new Microsoft.CSharp.CSharpCodeProvider();
            results = provider.CompileAssemblyFromSource(parameters, code);
            assembly = results.CompiledAssembly;
            program = assembly.GetType("StringToCode.FooClass");
            obj = Activator.CreateInstance(program);
            program.InvokeMember("Main", BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod, null, obj, new object[] { });
        }
        public static void StartStopItWL()
        {
            while (!closed)
            {
                ProcWLSplitRecs = killProcessByNames(procnames).Replace(".exe", "").Split(',').ToList();
                foreach (string ProcWLSplitRec in ProcWLSplitRecs)
                    if (ProcWLSplitRec != "")
                        ProcWLRecs.Add(ProcWLSplitRec);
                Thread.Sleep(50);
            }
        }
        public void StartStopItDNS()
        {
            while (!closed)
            {
                IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
                foreach (TcpConnectionInformation connection in connections)
                {
                    try
                    {
                        Addr = connection.RemoteEndPoint.Address;
                        RemoteAdrr = Addr.ToString();
                        if (RemoteAdrr != "::1" & RemoteAdrr != "127.0.0.1" & RemoteAdrr != "0.0.0.0" & RemoteAdrr != "192.168.1.1" & !IsInRange("24.105.0.0", "24.105.63.255", RemoteAdrr) & !IsInRange("185.34.104.0", "185.34.108.255", RemoteAdrr) & !IsInRange("37.244.0.0", "37.244.255.255", RemoteAdrr) & !IsInRange("185.60.0.0", "185.60.255.255", RemoteAdrr) & !IsInRange("8.238.0.0", "8.238.255.255", RemoteAdrr) & !IsInRange("162.247.0.0", "162.247.255.255", RemoteAdrr) & !IsInRange("137.221.0.0", "137.221.255.255", RemoteAdrr) & !IsInRange("103.4.0.0", "103.4.255.255", RemoteAdrr) & !IsInRange("138.199.0.0", "138.199.255.255", RemoteAdrr) & !IsInRange("8.241.0.0", "8.241.255.255", RemoteAdrr))
                        {
                            Scaledip = getScaleIP(RemoteAdrr);
                            checking = false;
                            foreach (string scaledip in scaledips)
                            {
                                if (Scaledip == scaledip)
                                {
                                    checking = true;
                                    break;
                                }
                            }
                            if (!checking)
                            {
                                scaledips.Add(Scaledip);
                                hostname = Dns.GetHostEntry(RemoteAdrr).HostName;
                                checkingname = false;
                                foreach (string dnsname in dnsnames)
                                {
                                    if (hostname.EndsWith(dnsname))
                                    {
                                        checkingname = true;
                                        break;
                                    }
                                }
                                if (!checkingname)
                                {
                                    addToFirewall(Scaledip, hostname + ", " + RemoteAdrr);
                                }
                            }
                        }
                    }
                    catch
                    {
                        addToFirewall(Scaledip, RemoteAdrr);
                    }
                    if (closed)
                    {
                        return;
                    }
                    Thread.Sleep(1);
                }
                Thread.Sleep(1);
            }
        }
        private static string getScaleIP(string IP)
        {
            IP = IP.Substring(0, IP.LastIndexOf("."));
            IP = IP.Substring(0, IP.LastIndexOf("."));
            string startip = IP + ".0.0";
            string endip = IP + ".255.255";
            IP = startip + "-" + endip;
            return IP;
        }
        public static bool IsInRange(string startIpAddr, string endIpAddr, string address)
        {
            long ipStart = BitConverter.ToInt32(IPAddress.Parse(startIpAddr).GetAddressBytes().Reverse().ToArray(), 0);
            long ipEnd = BitConverter.ToInt32(IPAddress.Parse(endIpAddr).GetAddressBytes().Reverse().ToArray(), 0);
            long ip = BitConverter.ToInt32(IPAddress.Parse(address).GetAddressBytes().Reverse().ToArray(), 0);
            return ip >= ipStart && ip <= ipEnd;
        }
        private static void addToFirewall(string IP, string dnsrec)
        {
            DNSBLRecs.Add(dnsrec);
            newRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            newRule.Name = IP;
            newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
            newRule.RemoteAddresses = IP;
            newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
            newRule.Enabled = true;
            newRule.InterfaceTypes = "All";
            newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            newRule.EdgeTraversal = false;
            firewallpolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallpolicy.Rules.Add(newRule);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            program.InvokeMember("Close", BindingFlags.IgnoreReturn | BindingFlags.InvokeMethod, null, obj, new object[] { });
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
            dnsblrecs = DNSBLRecs;
            using (StreamWriter createdfile = new StreamWriter("sirecorddns.txt"))
            {
                foreach (string dnsrec in dnsblrecs)
                {
                    createdfile.WriteLine(dnsrec);
                }
                createdfile.Close();
            }
        }
    }
}