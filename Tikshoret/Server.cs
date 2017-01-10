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
        bool rx = false;
        string hostName;
        string progName = "Networking17YHSC";

        public Server(UdpClient uc)
        {
            udpServer = uc;
            buildIpArray();
            buildTcpServer();
            buildUdpServer();
        }

        private void buildIpArray()
        {
            hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipArr = ipEntry.AddressList;
        }

        private void buildTcpServer()
        {
            int startPort = 6001, stopPort = 7000;
            availablePort = findAvailablePort(startPort, stopPort);
            //open tcp
            TcpListener listener = new TcpListener(IPAddress.Any,availablePort);
            listener.Start();
            Socket s = listener.AcceptSocket();
            EndPoint ep = s.RemoteEndPoint;
            Console.WriteLine("Connected to port {0} ip {1} compName {2}",s.RemoteEndPoint);
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
                        //take the number that the client sent in request message
                        byte[] offer = new byte[26];
                        byte[] rndNum = new byte[4];
                        rndNum[0] = data[16];
                        rndNum[1] = data[17];
                        rndNum[2] = data[18];
                        rndNum[3] = data[19];
                        //create the offer message
                        List<byte> msgList = new List<byte>();
                        string msgToSent = progName;
                        byte[] msg = System.Text.Encoding.UTF8.GetBytes(msgToSent);
                        msgList.AddRange(msg);
                        byte[] ipByteArr = System.Text.Encoding.UTF8.GetBytes(ipArr[3].ToString());
                        byte[] portByteArr = System.Text.Encoding.UTF8.GetBytes(availablePort.ToString());
                        msgList.AddRange(ipByteArr);
                        msgList.AddRange(portByteArr);
                        IPAddress broadcast = IPAddress.Parse("192.168.1.255");
                        groupEP.Address = broadcast;
                        groupEP.Port = 6000;
                        udpServer.Send(msg, msg.Length, groupEP);
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