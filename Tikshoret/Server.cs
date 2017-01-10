using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tikshoret
{
    class Server
    {
        UdpClient udpServer;
        TcpClient tcpServer;
        IPAddress[] ipArr;
        IPEndPoint groupEP;
        int availablePort = 0;
        byte[] data = new byte[1024];

        public Server(UdpClient uc)
        {
            udpServer = uc;
            buildIpArray();
            buildTcpServer();
            buildUdpServer();
        }

        private void buildIpArray()
        {
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipArr = ipEntry.AddressList;
        }

        private void buildTcpServer()
        {
            int startPort = 6001, stopPort = 7000;
            availablePort = findAvailablePort(startPort, stopPort);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, availablePort);
            tcpServer = new TcpClient(ep);
            //wait for a message
        }

        private static int findAvailablePort(int startPort, int stopPort)
        {
            IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConInfoArr = ipgp.GetActiveTcpConnections();
            Random r = new Random();
            var busyPorts = tcpConInfoArr.Select(t => t.LocalEndPoint.Port).Where(v => v >= startPort && v <= stopPort).ToArray();
            var firstAvailableRandomPort = Enumerable.Range(startPort, stopPort - startPort).OrderBy(v => r.Next()).FirstOrDefault(p => !busyPorts.Contains(p));
            return firstAvailableRandomPort;
        }

        private void buildUdpServer()
        {
            Console.WriteLine("Created server");
            groupEP = new IPEndPoint(IPAddress.Any, 6000);
            try
            {
                while (true)
                {
                    data = udpServer.Receive(ref groupEP);
                    if (!ipArr.Contains(groupEP.Address))
                    {
                        string msgRcvd = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                        Console.WriteLine("recieved {0}", msgRcvd);
                        string msgToSent = "pong";
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(msgToSent);
                        //check if any port of tcp in the range of 5000-6000 is currrently available
                        //if (checkIfTcpAvail())
                        //{
                        IPAddress broadcast = IPAddress.Parse("192.168.1.255");
                        groupEP.Address = broadcast;
                        groupEP.Port = 6000;
                        udpServer.Send(msg, msg.Length, groupEP);
                        //}
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Catch");
                Console.WriteLine(e.Message);
            }
            finally
            {
                udpServer.Close();
            }
        }

        private bool checkIfTcpAvail()
        {
            return false;
        }
    }
}