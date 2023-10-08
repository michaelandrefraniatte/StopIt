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

namespace StopIt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
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
        private static List<string> dnsnames = new List<string>(), ipranges = new List<string>(), DNSIPBLRecs = new List<string>(), dnsipblrecs = new List<string>();
        private static List<string> scaledips = new List<string>();
        private static string hostname = "";
        private static INetFwRule2 newRule;
        private static INetFwPolicy2 firewallpolicy;
        private static string RemoteAdrr = "0.0.0.0", Scaledip = "0.0.0.0";
        private static IPAddress Addr;
        private static bool checking, checkingname, checkingip;
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
            using (StreamReader file = new StreamReader("siwhitelistip.txt"))
            {
                while (true)
                {
                    string iprange = file.ReadLine();
                    if (iprange == "")
                    {
                        file.Close();
                        break;
                    }
                    else
                        ipranges.Add(iprange);
                }
            }
            Task.Run(() => StartStopItDNSIP());
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
        public void StartStopItDNSIP()
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
                        if (RemoteAdrr != "::1" & RemoteAdrr != "127.0.0.1" & RemoteAdrr != "0.0.0.0" & RemoteAdrr != "192.168.1.1")
                        {
                            checkingip = false;
                            foreach (string iprange in ipranges)
                            {
                                string[] ip = iprange.Split('-');
                                if (IsInRange(ip[0], ip[1], RemoteAdrr))
                                {
                                    checkingip = true;
                                    break;
                                }
                            }
                            if (!checkingip)
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
            DNSIPBLRecs.Add(dnsrec);
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
            closed = true;
            Thread.Sleep(200);
            string localDate = DateTime.Now.ToString();
            string date = localDate.Replace(" ", "-").Replace("/", "-").Replace(":", "-");
            procwlrecs = ProcWLRecs;
            using (StreamWriter createdfile = File.AppendText("sirecord.txt"))
            {
                createdfile.WriteLine(date);
                foreach (string procrec in procwlrecs)
                {
                    createdfile.WriteLine(procrec);
                }
                createdfile.Close();
            }
            dnsipblrecs = DNSIPBLRecs;
            using (StreamWriter createdfile = File.AppendText("sirecorddnsip.txt"))
            {
                createdfile.WriteLine(date);
                foreach (string dnsrec in dnsipblrecs)
                {
                    createdfile.WriteLine(dnsrec);
                }
                createdfile.Close();
            }
        }
    }
}