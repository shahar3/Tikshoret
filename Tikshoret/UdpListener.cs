﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Tikshoret
{
    /// <summary>
    /// This is an helper class 
    /// </summary>
    class UdpListener
    {
        UdpClient uc;

        public UdpListener(int port)
        {
            uc = new UdpClient(port);
        }

        public UdpClient getUdpListener()
        {
            return uc;
        }
    }
}
