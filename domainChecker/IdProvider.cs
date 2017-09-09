using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ipscan
{
    class IdProvider
    {
      
        private static int id = 0;
        public static int GetNewId()
        {

            id++;
            return id;
        }
    }
}
