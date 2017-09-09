using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ipscan
{
    class Proxy
    {
        public string ip;
        public int port;

        public Proxy(string ip, int port)
        {
            this.ip = ip;
            this.port = port;
        }
    }
}
