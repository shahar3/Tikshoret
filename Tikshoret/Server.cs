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
        #region fields
        UdpClient udpServer;
        static Socket s;
        static byte[] tcpData;
        IPAddress[] ipArr;
        IPEndPoint groupEP;
        Int16 availablePort = 0;
        byte[] data = new byte[1024];
        public static bool rx = false;
        string hostName;
        string progName = "Networking17YHSC";
        #endregion

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
            TcpListener listener = new TcpListener(IPAddress.Any, availablePort);
            listener.Start();
            //thread
            new Thread(() =>
            {
                s = listener.AcceptSocket();
                rx = true;
                s.Receive(tcpData);
                //rx=true && tx=false
                if (!Client.tx)
                {
                    //print the message to the screen
                    string msgToScreen = System.Text.Encoding.UTF8.GetString(tcpData);
                    Console.WriteLine(msgToScreen);
                }
                Console.WriteLine();
                //change the status
                EndPoint ep = s.RemoteEndPoint;
                Console.WriteLine("the server Connected to {0}", s.RemoteEndPoint);
            }).Start();
            //wait for a message
        }





        private static Int16 findAvailablePort(int startPort, int stopPort)
        {
            IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConInfoArr = ipgp.GetActiveTcpConnections();
            Random r = new Random();
            var busyPorts = tcpConInfoArr.Select(t => t.LocalEndPoint.Port).Where(v => v >= startPort && v <= stopPort).ToArray();
            var firstAvailableRandomPort = Enumerable.Range(startPort, stopPort - startPort).OrderBy(v => r.Next()).FirstOrDefault(p => !busyPorts.Contains(p));
            return (Int16)firstAvailableRandomPort;
        }

        private void buildUdpServer()
        {
            Console.WriteLine("Created server");
            groupEP = new IPEndPoint(IPAddress.Any, 5999);
            try
            {
                while (!rx)
                {
                    data = udpServer.Receive(ref groupEP);

                    if (!ipArr.Contains(groupEP.Address) && data.Length == 20)
                    {

                        string msgRcvd = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                        Console.WriteLine("server recieved {0} length {1}", msgRcvd, data.Length);
                        //take the number that the client sent in request message
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
                        msgList.AddRange(rndNum);
                        IPAddress ip = ipArr[3];
                        byte[] ipByteArr = ip.GetAddressBytes();
                        byte[] portByteArr = BitConverter.GetBytes(availablePort);
                        msgList.AddRange(ipByteArr);
                        msgList.AddRange(portByteArr);
                        msg = msgList.ToArray();
                        IPAddress broadcast = IPAddress.Parse("192.168.1.255");
                        groupEP.Address = broadcast;
                        groupEP.Port = 5999;
                        udpServer.Client.SendTo(msg, groupEP);
                        Console.WriteLine("send offer");
                        rx = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Catch");
                Console.WriteLine(e.Message);
            }
        }

        private bool checkIfTcpAvail()
        {
            return false;
        }

        /// <summary>
        /// get the msg tcp data and change it
        /// </summary>
        public static string malfunctionMsg()
        {
            Random r = new Random();
            int charIndex = r.Next(tcpData.Length);
            char rndChar = (char)r.Next(32, 127);
            string tcpdataString = System.Text.Encoding.UTF8.GetString(tcpData);
            char[] charStr = tcpdataString.ToCharArray();
            charStr[charIndex] = rndChar;
            string newString = new string(charStr);
            Console.WriteLine(newString);
            return newString;
        }
    }
}