using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
namespace MockTcpListener
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 555;
            TcpListener listener=new TcpListener(IPAddress.Any,port); 
            listener.Start();
            Console.WriteLine("Listener started on tcp port: "+ port);
            Console.WriteLine("Press any key to quit.");
            Console.Read();
            listener.Stop();
        }
    }
}
