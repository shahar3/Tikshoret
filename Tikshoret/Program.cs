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
    /// <summary>
    /// This class run the the threads that execute the server and the client
    /// </summary>
    class Program
    {

        static void Main(string[] args)
        {
            introduction();
            UdpListener ul = new UdpListener(6000);
            UdpClient udpReceiver = ul.getUdpListener();
            //open client
            Thread t = new Thread(() =>
            {
                openClient(udpReceiver);
            });
            //open server
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

        /// <summary>
        /// this function print to the screen the intrudction of the program
        /// </summary>
        private static void introduction()
        {
            Console.WriteLine("****************************************************");
            Console.WriteLine("****       Welcome to the broken phone app      ****");
            Console.WriteLine("****************************************************");
        }

        /// <summary>
        /// this function build the server side 
        /// </summary>
        /// <param name="uc"></param>
        private static void openServer(UdpClient uc)
        {
            Server s = new Server(uc);
        }
        /// <summary>
        /// this function build the client side
        /// </summary>
        /// <param name="uc"></param>
        private static void openClient(UdpClient uc)
        {
            Client c = new Client(uc);
        }
    }
}
