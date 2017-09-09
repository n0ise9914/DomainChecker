using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ipscan
{
    class Worker
    {

        public delegate void OnRetryDelegate(String domain);
        public event OnRetryDelegate OnRetry;


        public delegate void OnRespnseDelegate(Worker sender, bool success);
        public event OnRespnseDelegate OnRespnse;

        public Worker()
        {
            Id = IdProvider.GetNewId();
            thread = new Thread(new ThreadStart(ExecuteAsync));
            thread.Start();
        }

        private List<int> ports;
        private Thread thread;
        public string ip;
        private int timeOut;
        public bool Idle { get; set; }
        public bool ShutDown { get; set; }
        public bool Started { get; set; }
        public int Id { get; private set; }
        public Proxy proxy { get; private set; }
        public void Start(String ip, List<int> ports, int timeOut,Proxy proxy)
        {
            this.proxy = proxy;
            this.ip = ip;
            this.ports = ports;
            this.timeOut = timeOut;
            Idle = true;
            Started = true;
        }
 
        private void ExecuteAsync()
        {
            while (!ShutDown)
            {
                Thread.Sleep(100);
                if (Idle)
                {
                    Idle = false;
                    if (ip == null)
                    {
                        ShutDown = true;
                        return;
                    }
                    foreach (var port in ports)
                    {
                        try
                        {
                         
                            Debug.Print(ip);
                            HttpWebRequest request = WebRequest.Create(ip) as HttpWebRequest;
                            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.2 (KHTML, like Gecko) Chrome/15.0.874.121 Safari/535.2";
                            request.Proxy = new WebProxy(proxy.ip, proxy.port);
                            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                            WebHeaderCollection header = response.Headers;
                            var encoding = ASCIIEncoding.ASCII;
                            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
                            {
                                string responseText = reader.ReadToEnd();
                                Debug.Print(responseText);
                                // Console.ReadLine();
                                if (responseText.Contains("ERROR:101"))
                                {

                                    OnRespnse?.Invoke(this, true);
                                }

                            }
                        }
                        catch (Exception)
                        {
                            OnRetry?.Invoke(ip);

                        }
                       
                         
                    }
                        
                    OnRespnse?.Invoke(this, false);
                }
            }
        }
    }
}
