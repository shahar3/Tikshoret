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
        #region fields
        byte[] received = null;
        UdpClient udpSender;
        UdpClient udpReciever;
        IPAddress[] ipArr;
        public static bool tx = false;
        byte[] byteToSend;
        string portToConnectTcp;
        TcpClient client;
        string requestMessage;
        byte[] offer = null;
        public static Mutex m;
        #endregion


        public Client(UdpClient uc)
        {
            udpSender = new UdpClient();
            udpReciever = uc;
            string compName = Dns.GetHostName();
            IPHostEntry compIp = Dns.GetHostEntry(compName);
            Console.WriteLine("Comp name: {0}", compName);
            IPAddress broadcast = IPAddress.Parse("192.168.1.255");
            IPEndPoint ep = new IPEndPoint(broadcast, 5999);
            IPEndPoint recieveEP = new IPEndPoint(IPAddress.Any, 5999);
            Console.WriteLine("created client");
            udpSender.Connect(ep);
            //
            string hostName = Dns.GetHostName();
            IPHostEntry ipEntry = Dns.GetHostEntry(hostName);
            ipArr = ipEntry.AddressList;
            new Thread(() =>
            {
                while (!tx && !Server.rx)
                {
                    received = udpReciever.Receive(ref recieveEP);
                    if (!ipArr.Contains(recieveEP.Address) && !Server.rx)
                    {
                        Program.m.WaitOne();
                        if (received.Length == 26 && !Server.rx)
                        {
                            //change the status
                            tx = true;
                            offer = received;
                            portToConnectTcp = System.Text.Encoding.UTF8.GetString(received, 0, received.Length);
                            Console.WriteLine("Offer msg receieved from the server " + portToConnectTcp);
                        }
                        Program.m.ReleaseMutex();
                    }
                }
            }).Start();
            //create the request msg
            createRequestMsg();
            while (!tx && !Server.rx)
            {
                Program.m.WaitOne();
                //send the message to the server
                udpSender.Send(byteToSend, byteToSend.Length);
                Console.WriteLine("The client send the requst message");
                Thread.Sleep(800);
                Program.m.ReleaseMutex();
            }
            //now we have the connect details to tcp
            if (tx)
            {
                connectToTcp();
            }
        }

        private void createRequestMsg()
        {
            //create the request message
            Random rnd = new Random();
            int randomNumtoSend = rnd.Next();
            byte[] rndNumByte = BitConverter.GetBytes(randomNumtoSend);
            List<byte> byteToSendList = new List<byte>();
            requestMessage = "Networking17YHSC";
            //convert the message to bytes array
            byteToSend = System.Text.Encoding.UTF8.GetBytes(requestMessage);
            byteToSendList.AddRange(byteToSend);
            byteToSendList.AddRange(rndNumByte);
            byteToSend = byteToSendList.ToArray();
        }

        private void connectToTcp()
        {
            //seperate the string with the details to ip and port
            //get the number
            byte[] rndNum = new byte[4];
            rndNum[0] = offer[16];
            rndNum[1] = offer[17];
            rndNum[2] = offer[18];
            rndNum[3] = offer[19];
            //get the ip
            byte[] ip = new byte[4];
            ip[0] = offer[20];
            ip[1] = offer[21];
            ip[2] = offer[22];
            ip[3] = offer[23];
            IPAddress ipAddress = new IPAddress(ip);
            //get the port
            byte[] portA = new byte[2];
            portA[0] = offer[24];
            portA[1] = offer[25];
            int port = BitConverter.ToInt16(portA, 0);
            //connect to the TCP
            client = new TcpClient();
            client.Connect(ipAddress, port);
            Console.WriteLine("The client connected to the Tcp");
            //
            checkTheStatus();
        }

        private void checkTheStatus()
        {
            //rx=false && tx=true
            if (!Server.rx && tx)
            {
                string input = getInputFromTheUser();
                //convert to byte array
                byte[] sendToTheServer = Encoding.ASCII.GetBytes(input);
                //send to the Tcp server
                client.Client.Send(sendToTheServer);
            }
            //rx=true && tx=true
            if (Server.rx && tx)
            {
                string msgToServer = Server.malfunctionMsg();
                byte[] sendToServer = Encoding.ASCII.GetBytes(msgToServer);
                client.Client.Send(sendToServer);
            }
        }

        private string getInputFromTheUser()
        {
            Console.WriteLine("please enter input: ");
            return Console.ReadLine();
        }
    }
}