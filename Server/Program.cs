using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sever
{
    class Program
    {
        const int PORT = 11000;
        static void Main(string[] args)
        {
            Server server = new Server(PORT);
            server.Start();
            Console.ReadKey();
        }
    }
}
