using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using NetFwTypeLib;
using System.Net.NetworkInformation;
namespace StopIt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("stopitnames.dll", EntryPoint = "processnames")]
        [return: MarshalAs(UnmanagedType.BStr)]
        public static extern string processnames();
        [DllImport("stopitkill.dll", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "killProcessByName")]
        public static extern void killProcessByName(string processname);

        [DllImport("winmm.dll", EntryPoint = "timeBeginPeriod")]
        public static extern uint TimeBeginPeriod(uint ms);
        [DllImport("winmm.dll", EntryPoint = "timeEndPeriod")]
        public static extern uint TimeEndPeriod(uint ms);
        [DllImport("ntdll.dll", EntryPoint = "NtSetTimerResolution")]
        public static extern void NtSetTimerResolution(uint DesiredResolution, bool SetResolution, ref uint CurrentResolution);
        public static uint CurrentResolution = 0;
        public static int processid = 0;
        private static List<string> procWLs = new List<string>(), ProcWLRecs = new List<string>(), procwlrecs = new List<string>(), processes = new List<string>(), ProcCatchRecs = new List<string>(), proccatchrecs = new List<string>(), dnsnames = new List<string>(), DNSBLRecs = new List<string>(), dnsblrecs = new List<string>();
        private static string procnames;
        private static bool processfound;
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
                        procWLs.Add(procName);
                }
            }
            Task.Run(() => StartStopItWL());
            using (StreamReader file = new StreamReader("sidns.txt"))
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
        }
        public static void StartStopItWL()
        {
            while (!closed)
            {
                procnames = processnames();
                procnames = procnames.Remove(procnames.Length - 1);
                procnames = procnames.Substring(1);
                procnames = procnames.Replace(".exe", "");
                processes = procnames.Split(',').ToList();
                foreach (string process in processes)
                {
                    if (process != "StopIt" & process != "svchost" & process != "Secure System" & process != "System" & process != "Idle" & process != "Memory Compression" & process != "csrss" & process != "dwm" & process != "lsass" & process != "ntoskrnl" & process != "smss" & process != "explorer" & process != "wininit" & process != "winlogon" & process != "LsaIso" & process != "LogonUI" & process != "SecurityHealthService" & process != "StartMenuExperienceHost" & process != "sihost" & process != "fontdrvhost" & process != "audiodg" & process != "ctfmon" & process != "WmiPrvSE" & process != "SecurityHealthHost" & process != "smartscreen" & process != "conhost" & process != "Call of Duty Launcher" & process != "cod" & process != "codCrashHandler" & process != "ReportCodBug" & process != "bootstrapper" & process != "bootstrapperCrashHandler" & process != "BlizzardBrowser" & process != "Battle.net" & process != "Battle.net Launcher" & process != "Agent" & process != "consent" & process != "Taskmgr" & process != "LaunchTM" & process != "mmc" & process != "perfmon" & process != "Registry" & process != "services" & process != "MsMpEng" & process != "NisSrv" & process != "SgrmBroker" & process != "MpCmdRun" & process != "LockApp" & process != "ciine")
                    {
                        processfound = false;
                        foreach (string procWL in procWLs)
                        {
                            if (process == procWL)
                            {
                                processfound = true;
                                break;
                            }
                        }
                        if (!processfound)
                        {
                            try
                            {
                                killProcessByName(process + ".exe");
                                ProcWLRecs.Add(process);
                            }
                            catch
                            {
                                ProcCatchRecs.Add(process);
                            }
                        }
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
                        if (RemoteAdrr != "::1" & RemoteAdrr != "127.0.0.1" & RemoteAdrr != "0.0.0.0" & RemoteAdrr != "192.168.1.1" & !IsInRange("24.105.0.0", "24.105.63.255", RemoteAdrr) & !IsInRange("185.34.104.0", "185.34.108.255", RemoteAdrr) & !IsInRange("37.244.0.0", "37.244.255.255", RemoteAdrr) & !IsInRange("95.179.0.0", "95.179.255.255", RemoteAdrr) & !IsInRange("185.60.0.0", "185.60.255.255", RemoteAdrr) & !IsInRange("8.238.0.0", "8.238.255.255", RemoteAdrr) & !IsInRange("162.247.0.0", "162.247.255.255", RemoteAdrr) & !IsInRange("137.221.0.0", "137.221.255.255", RemoteAdrr))
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
                                if (!hostname.EndsWith(".r.cloudfront.net") & !hostname.EndsWith(".1e100.net"))
                                {
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
            newRule = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FWRule"));
            newRule.Name = IP;
            newRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
            newRule.RemoteAddresses = IP;
            newRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
            newRule.Enabled = true;
            newRule.InterfaceTypes = "All";
            newRule.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
            newRule.EdgeTraversal = false;
            firewallpolicy = (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
            firewallpolicy.Rules.Add(newRule);
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            Thread.Sleep(200);
            proccatchrecs = ProcCatchRecs;
            using (StreamWriter createdfile = new StreamWriter("sicatchrecord.txt"))
            {
                foreach (string procrec in proccatchrecs)
                {
                    createdfile.WriteLine(procrec);
                }
                createdfile.Close();
            }
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
            using (StreamWriter createdfile = new StreamWriter("sidnsrecord.txt"))
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