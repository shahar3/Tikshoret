﻿using System;
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
    /// <summary>
    /// this class represents the server side
    /// </summary>
    class Server
    {

        #region fields
        UdpClient udpServer;
        static Socket s;
        static byte[] tcpData = new byte[1024];
        IPAddress[] ipArr;
        IPEndPoint groupEP;
        Int16 availablePort = 0;
        byte[] data = new byte[1024];
        public static bool rx = false;
        public static bool tcpRecv = false;
        string hostName;
        string progName = "Networking17YYYY";
        #endregion

        /// <summary>
        /// this constructor receive udp client object from the main
        /// </summary>
        /// <param name="uc"></param>
        public Server(UdpClient uc)
        {
            udpServer = uc;
            buildIpArray();
            buildTcpServer();
            buildUdpServer();
        }

        /// <summary>
        /// get the details of the computer
        /// </summary>
        private void buildIpArray()
        {
            hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipArr = ipEntry.AddressList;
        }

        /// <summary>
        /// this function create listen on tcp
        /// </summary>
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
                Console.WriteLine("{0} connected", s.RemoteEndPoint);
                rx = true;
                s.Receive(tcpData);
                tcpRecv = true;
                //rx=true && tx=false
                //print the message to the screen
                string msgToScreen = System.Text.Encoding.UTF8.GetString(tcpData);
                Console.WriteLine(msgToScreen);
                Console.WriteLine();
                //change the status
                EndPoint ep = s.RemoteEndPoint;
            }).Start();
        }

        /// <summary>
        /// This function find available port between 6000-7000 
        /// </summary>
        /// <param name="startPort"></param>
        /// <param name="stopPort"></param>
        /// <returns></returns>
        private static Int16 findAvailablePort(int startPort, int stopPort)
        {
            IPGlobalProperties ipgp = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConInfoArr = ipgp.GetActiveTcpConnections();
            Random r = new Random();
            var busyPorts = tcpConInfoArr.Select(t => t.LocalEndPoint.Port).Where(v => v >= startPort && v <= stopPort).ToArray();
            var firstAvailableRandomPort = Enumerable.Range(startPort, stopPort - startPort).OrderBy(v => r.Next()).FirstOrDefault(p => !busyPorts.Contains(p));
            return (Int16)firstAvailableRandomPort;
        }

        /// <summary>
        /// This function create a listen on udp with port 6000 
        /// waiting for request message and return offer message that
        /// contain details about the tcp connection 
        /// </summary>
        private void buildUdpServer()
        {
            Console.WriteLine("Created server");
            groupEP = new IPEndPoint(IPAddress.Any, 6000);
            try
            {
                while (!rx)
                {

                    data = udpServer.Receive(ref groupEP);
                    if (!ipArr.Contains(groupEP.Address) && data.Length == 20)
                    {
                        string msgRcvd = System.Text.Encoding.UTF8.GetString(data, 0, data.Length);
                        Console.WriteLine("Request message received");
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
                        IPAddress ip = IPAddress.Parse(getIp(NetworkInterfaceType.Wireless80211));
                        byte[] ipByteArr = ip.GetAddressBytes();
                        byte[] portByteArr = BitConverter.GetBytes(availablePort);
                        msgList.AddRange(ipByteArr);
                        msgList.AddRange(portByteArr);
                        msg = msgList.ToArray();
                        IPAddress broadcast = IPAddress.Parse("192.168.1.255");
                        groupEP.Address = broadcast;
                        groupEP.Port = 6000;
                        udpServer.Send(msg, msg.Length, groupEP);
                        Console.WriteLine("sent offer request- port {0} ip {1}", availablePort, ip.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Catch");
                Console.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Get the V4 ip from array of all ip's of the computer
        /// </summary>
        /// <param name="_type"></param>
        /// <returns></returns>
        private string getIp(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
            return output;
        }


        /// <summary>
        /// get the msg tcp data and change it
        /// </summary>
        public static string malfunctionMsg()
        {
            string tcpdataString = System.Text.Encoding.UTF8.GetString(tcpData);
            Random r = new Random();
            int charIndex = r.Next(tcpdataString.Trim('\0').Length);
            char rndChar = (char)r.Next(32, 127);
            char[] charStr = tcpdataString.ToCharArray();
            charStr[charIndex] = rndChar;
            string newString = new string(charStr);
            Console.WriteLine(newString);
            return newString;
        }
    }
}