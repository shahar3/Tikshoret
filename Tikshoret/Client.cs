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
    class Client
    {
        #region sockets
        byte[] received = null;
        UdpClient udpSender;
        UdpClient udpReciever;
        IPAddress[] ipArr;
        #endregion


        public Client(UdpClient uc)
        {
            udpSender = new UdpClient();
            udpReciever = uc;
            string compName = Dns.GetHostName();
            IPHostEntry compIp = Dns.GetHostEntry(compName);
            Console.WriteLine("Comp name: {0}", compName);
            IPAddress broadcast = IPAddress.Parse("192.168.1.255");
            IPEndPoint ep = new IPEndPoint(broadcast, 6000);
            IPEndPoint recieveEP = new IPEndPoint(IPAddress.Any, 6000);
            Console.WriteLine("created client");
            udpSender.Connect(ep);
            //
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipArr = ipEntry.AddressList;
            new Thread(() =>
            {
                while (received == null)
                {
                    received = udpReciever.Receive(ref recieveEP);
                    if (!ipArr.Contains(recieveEP.Address))
                    {
                        string portToConnectTcp = System.Text.Encoding.UTF8.GetString(received, 0, received.Length);
                        Console.WriteLine(portToConnectTcp);
                        //
                    }
                    else
                    {
                        received = null;
                    }
                }
            }).Start();
            while (received == null)
            {
                string toSend = "Hello shahar!";
                byte[] byteToSend = System.Text.Encoding.UTF8.GetBytes(toSend);
                udpSender.Send(byteToSend, byteToSend.Length);
                Thread.Sleep(1000);
            }
            Console.WriteLine("Finished");
        }
    }
}