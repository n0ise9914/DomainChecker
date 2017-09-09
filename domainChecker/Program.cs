using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ipscan
{


    class Program
    {
     
        static int  LineTop, LineLeft;
        static int threads = 40;
        static int timeOut = 1000;
        static int successed = 0, failed = 0;
       // static int totalRangesCount;
       // static  string file = ;
        static List<int> ports = new List<int>();
        private static List<string> goods = new List<string>();


        static void check(String ip)
        {
     
        



            HttpWebRequest request = WebRequest.Create("http://whois.nic.ir/?name=" + ip + ".ir") as HttpWebRequest;
            request.Proxy = new WebProxy("168.128.29.75", 80);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            WebHeaderCollection header = response.Headers;
            var encoding = ASCIIEncoding.ASCII;
            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
            {
                string responseText = reader.ReadToEnd();
                if (responseText.Contains("ERROR:101"))
                    Console.WriteLine("ERROR:101");
            }
            Console.WriteLine("no error");


               Console.ReadLine();
        }
        static void Main(string[] args)
        {
            //check("gregregregre");
            Console.BackgroundColor = ConsoleColor.Black;
            if (args.Length==-1)
            {
                LineTop = 2;
                LineLeft = 5;
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("No params!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            ports.Add(80);
           // 3389,3390,3391
            Console.CursorVisible = false;
            Console.Clear();
            for (int i = 0;i<args.Length;i++)
            {
                switch(args[i])
                {
                    case "-t":
                        threads = Convert.ToInt32(args[i + 1].Trim());
                        break;
                    case "-o":
                        timeOut = Convert.ToInt32(args[i + 1].Trim());
                        break;
                    case "-f":
                       // file = args[i + 1].Trim();
                        break;
                    case "-p":
                        foreach (string port in args[i + 1].Trim().Split(','))
                        {
                            ports.Add(Convert.ToInt32(port));
                        }
                        break;

                }
            }
     
            List<string> ranges= LoadRangesFromFile(@"domains.txt");
            List<Proxy> proxies = LoadProxies(@"proxies.txt");


       
            Scanner scanner = new Scanner();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;

            string strPorts = "|";
            foreach (var port in ports)
                strPorts += port+"|";


            int total = ranges.Count;
            timer.Elapsed += (source, e) =>
            {
                SaveGoodHosts();

                LineTop = 2;
                LineLeft = 5;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format("================== Total =================="));
                WriteLine(string.Format("Threads:    {0}", scanner.workers.Count));
                WriteLine(string.Format("Ports:      {0}", strPorts));
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                WriteLine(string.Format("TimeOut:    {0}", timeOut));
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                WriteLine(string.Format("Domains:    {0}", total));
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format("Remaining:  {0}", scanner.Domains.Count ));
                Console.ForegroundColor = ConsoleColor.DarkGreen;    
                WriteLine(string.Format("Available:  {0}", successed));
                Console.ForegroundColor = ConsoleColor.DarkRed;
                WriteLine(string.Format("Retries:    {0}", scanner.retries));
                WriteLine(string.Format("Registered: {0}", failed));
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format(""));
 
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                if (finished)
                {
                    WriteLine("");
                    WriteLine("Finished [!]");
                }
                
                WriteLine("                                          ");
                WriteLine("                                          ");
            };
            timer.Start();

            scanner.OnRespnse += (ip, success) =>
            {
      
                if (success)
                {
                    goods.Add(ip);
                    successed++;
                }
                else failed++;

            };
            scanner.OnFinished += () =>
            {
                Debug.Print("finished");
                if (!finished) finished = true;  
            };
            scanner.TimeOut = timeOut;
            scanner.Domains = ranges;
            scanner.Proxies = proxies;
            scanner.Threads = threads;
            scanner.Ports = ports;
            scanner.Start();

            Console.ReadLine();
        }

        private static void SaveGoodHosts()
        {
            string lines = null;
            while (goods.Count != 0)
            {

                lines += goods[0] + Environment.NewLine;
                goods.RemoveAt(0);
            }
            if (lines != null)
            using (StreamWriter w = File.AppendText("available.txt"))
            {
                w.Write(lines);
                w.Flush();
            }
        }

        static bool finished = false;
        public static void WriteLine(string[] lines)
        {
            foreach (var line in lines)
            {
                WriteLine(line);
            }
        }
        public static void WriteLine(string lines)
        {
            Console.SetCursorPosition(LineLeft, LineTop);
            Console.Write(lines+"                   ");
            LineTop++;
        }

        public static void ClearCurrentConsoleLine()
        {
    
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        private static List<string> LoadRangesFromFile(string path)
        {
        
            List<string> ranges = new List<string>();
            string text = File.ReadAllText(path, Encoding.UTF8);
            string[] strRanges = text.Split('\n');
            foreach (string range in strRanges)
            {
   
                ranges.Add("http://whois.nic.ir/?name=" + range.Trim() + ".ir");
            }
            return ranges;
        }

        private static List<Proxy> LoadProxies(string path)
        {

            List<Proxy> ranges = new List<Proxy>();
            string text = File.ReadAllText(path, Encoding.UTF8);
            string[] strRanges = text.Split('\n');
            foreach (string range in strRanges)
            {
                String ip = range.Substring(0, range.Trim().IndexOf(":"));
                int port = Convert.ToInt32(range.Substring(range.IndexOf(":") + 1));
                ranges.Add(new Proxy(ip,port));
            }
            return ranges;
        }
    }
}
