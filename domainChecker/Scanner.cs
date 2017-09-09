using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ipscan
{
    class Scanner
    {
        public delegate void OnRespnseDelegate(string ip, bool success);
        public event OnRespnseDelegate OnRespnse;
        public delegate void OnFinishedDelegate();
        public event OnFinishedDelegate OnFinished;
        public List<Worker> workers = new List<Worker>();
        private Object ThreadLock = new object();
    
        private List<string> domains;
        public List<string> Domains
        {
            get
            {
                return domains;
            }
            
            set
            {
                domains = value;
            }
        }
  
        public int Threads { get; set; }
        public List<int> Ports { get; set; }
        public int TimeOut { get; set; }
        private Timer timer = new Timer();
        public Scanner()
        {
            timer.Interval = 500;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private bool started;
        public List<Proxy> Proxies { get; set; }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (workers.Count == Threads)
            {
                //start workers
                if (!started) StartWorkers();
                //dispose workers
                DisposeWorkers();           
            }
        }

        private void DisposeWorkers()
        {
            int count = 0;
            foreach (Worker worker in workers)
                if (worker.ShutDown) count++;
            if (count == Threads)
            {
                timer.Stop();
                OnFinished?.Invoke();
            }
        }

        private void StartWorkers()
        {
            int count = 0;
            started = true;
            foreach (Worker worker in workers)
                if (!worker.Started) count++;
            if (count == Threads)
            {
                foreach (var worker in workers)
                {
                    Debug.Print("new worker: " + worker.Id);
                    worker.Start(GetIp(), Ports, TimeOut, GetProxy());
                }
            }
        }
        public int retries;
        public void Start()
        { 
            //create workers       
            for (int i = 0; i < Threads; i++)
            {
                Worker worker = new Worker();
                workers.Add(worker);
                worker.OnRetry += (domain) =>
                {
                    retries++;
                    domains.Add(domain);
                };
                worker.OnRespnse += (sender, success) =>
                {
                    Debug.Print("response " + sender.ip);
                    OnRespnse?.Invoke(sender.ip, success);
                    string ip = GetIp();
                    Proxy proxy = GetProxy();
                    worker.Start(ip, Ports, TimeOut,GetProxy());
                };
            }
  
        }

        int proxyIndex;
        private Proxy GetProxy()
        {
            return Proxies[proxyIndex];

            if (proxyIndex == Proxies.Count)
                proxyIndex = 0;
            else
            proxyIndex++;

       

        }

        public string GetIp()
        {   
     
               if (Domains.Count != 0)
                {
                   string word = Domains[0];
                   Domains.RemoveAt(0);
                  return word;
                }

            return null;
        }
    }
}
