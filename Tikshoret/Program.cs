using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tikshoret
{
    class Program
    {
        static void Main(string[] args)
        {
            UdpListener ul = new UdpListener(6000);
            UdpClient udpReceiver = ul.getUdpListener();
            Thread t = new Thread(() =>
            {
                openClient(udpReceiver);
            });
            Thread t2 = new Thread(() =>
            {
                openServer(udpReceiver);
            });
            t2.Start();
            Thread.Sleep(20);
            t.Start();
            Console.ReadKey();
        }



        private static void openServer(UdpClient uc)
        {
            Server s = new Server(uc);
        }

        private static void openClient(UdpClient uc)
        {
            Client c = new Client(uc);
        }
    }
}
