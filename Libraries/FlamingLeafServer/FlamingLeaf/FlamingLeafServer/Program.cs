using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlamingLeaf.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var fsServer = new FlamingLeafServer(27020);
            fsServer.RunServer();
        }
    }
}
