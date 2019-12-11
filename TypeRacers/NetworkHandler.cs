﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TypeRacers.Client;

namespace TypeRacers
{
    //a class that handles the messages to and from the network
    public class NetworkHandler : INetworkHandler
    {
        TypeRacersClient client;
        public NetworkHandler()
        {
            client = new TypeRacersClient();
        }

        public string GetMessageFromServer()
        {
            return client.GetMessageFromServer();
        }

        public void SendProgressToServer(string progress)
        {
            client.SendProgressToServer(progress);
        }
    }
}