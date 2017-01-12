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
            introduction();
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
            Thread t3 = new Thread(() =>
            {
                while (true)
                {
                    Console.WriteLine("tx: {0} \trx: {1}", Client.tx, Server.rx);
                    Thread.Sleep(10000);
                }
            });
            t2.Start();
            t.Start();
            t3.Start();
            while (!Server.rx)
            {

            }
            Console.ReadKey();
        }

        private static void introduction()
        {
            Console.WriteLine("****************************************************");
            Console.WriteLine("****       Welcome to the broken phone app      ****");
            Console.WriteLine("****************************************************");
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
