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
    /// this class represents the client side
    /// </summary>
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
        public static TcpClient client;
        string requestMessage;
        byte[] offer = null;
        public static Mutex m;
        int TIMEOUT = 60;
        const string networkName = "Networking17YHSC";
        #endregion

        /// <summary>
        /// this constructor receive udp client object from the main
        /// get the details about this computer.
        /// configure the udp broadcast
        /// </summary>
        /// <param name="uc"></param>
        public Client(UdpClient uc)
        {
            udpSender = new UdpClient();
            udpReciever = uc;
            string compName = Dns.GetHostName();
            IPHostEntry compIp = Dns.GetHostEntry(compName);
            IPAddress broadcast = IPAddress.Parse("192.168.1.255");
            IPEndPoint ep = new IPEndPoint(broadcast, 6000);
            IPEndPoint recieveEP = new IPEndPoint(IPAddress.Any, 6000);
            printDetails(compName, TIMEOUT, 6000);
            udpSender.Connect(ep);
            IPHostEntry ipEntry = Dns.GetHostEntry(compName);
            ipArr = ipEntry.AddressList;
            //receive offer message from the server
            receiveOfferMessage(recieveEP);
            //create the request msg
            createRequestMsg();
            //send request message to the servers
            //this function work until we get offer message from some server
            sendRequestMessage();
            //now we have the connect details to tcp
            if (tx && !Server.rx)
            {
                connectToTcp();
            }
        }

        /// <summary>
        /// This function send request message to the servers on broadcast each second
        /// </summary>
        private void sendRequestMessage()
        {
            while (!tx && !Server.rx && TIMEOUT > 0)
            {
                if (!tx && !Server.rx)
                {
                    //send the message to the server
                    udpSender.Send(byteToSend, byteToSend.Length);
                    Thread.Sleep(1000);
                    TIMEOUT--;
                }
            }
        }

        /// <summary>
        /// This function waiting for offer message from some server
        /// </summary>
        /// <param name="recieveEP"></param>
        private void receiveOfferMessage(IPEndPoint recieveEP)
        {
            new Thread(() =>
            {
                while (!tx && !Server.rx)
                {
                    received = udpReciever.Receive(ref recieveEP);
                    if (!ipArr.Contains(recieveEP.Address))
                    {
                        if (received.Length == 26)
                        {
                            //change the status
                            if (!Server.rx)
                                tx = true;
                            offer = received;
                            portToConnectTcp = System.Text.Encoding.UTF8.GetString(received, 0, received.Length);
                            Console.WriteLine("Offer msg receieved from the server " + portToConnectTcp);
                        }
                    }
                }
            }).Start();
        }

        /// <summary>
        /// this function print details about our situation
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="timeout"></param>
        /// <param name="udpPort"></param>
        private void printDetails(string hostName, int timeout, int udpPort)
        {
            Console.WriteLine();
            Console.WriteLine("Host name: {0} \t Udp port listening to {1} \t set timeout to {2} seconds", hostName, udpPort, timeout);
            Console.WriteLine("-------------------------------------------------------------------------------");
            Console.WriteLine("Our network name: {0}", networkName);
            Console.WriteLine();
        }

        /// <summary>
        /// create request message
        /// </summary>
        private void createRequestMsg()
        {
            //create the request message
            Random rnd = new Random();
            int randomNumtoSend = rnd.Next();
            byte[] rndNumByte = BitConverter.GetBytes(randomNumtoSend);
            List<byte> byteToSendList = new List<byte>();
            requestMessage = networkName;
            //convert the message to bytes array
            byteToSend = System.Text.Encoding.UTF8.GetBytes(requestMessage);
            byteToSendList.AddRange(byteToSend);
            byteToSendList.AddRange(rndNumByte);
            byteToSend = byteToSendList.ToArray();
        }

        /// <summary>
        /// connect to tcp
        /// </summary>
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
            client.ConnectAsync(ipAddress, port);
            checkTheStatus();
            while (true) ;
        }

        /// <summary>
        /// check our status 
        /// rx=false,tx=true
        /// or
        /// rx=true,tx=true
        /// </summary>
        private void checkTheStatus()
        {
            //rx=false && tx=true
            Thread t = new Thread(rx_off_tx_on);
            if (!Server.rx && tx)
            {
                t.Start();
            }
            //rx=true && tx=true
            while (true)
            {
                if (Server.rx && tx)
                {
                    t.Abort();
                    while (!Server.tcpRecv) ;
                    string msgToServer = Server.malfunctionMsg();
                    byte[] sendToServer = Encoding.ASCII.GetBytes(msgToServer);
                    client.Client.Send(sendToServer);
                    break;
                }
            }
        }

        /// <summary>
        /// In this status we get a string from the user and transfer to the server
        /// </summary>
        private void rx_off_tx_on()
        {
            string input = getInputFromTheUser();
            //convert to byte array
            byte[] sendToTheServer = Encoding.ASCII.GetBytes(input);
            //send to the Tcp server
            client.Client.Send(sendToTheServer);
        }

        /// <summary>
        /// This function get string from the user
        /// </summary>
        /// <returns></returns>
        private string getInputFromTheUser()
        {
            Console.WriteLine("please enter input: ");
            return Console.ReadLine();
        }
    }
}