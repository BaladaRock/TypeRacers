﻿using System;
using System.Net;
using System.Net.Sockets;

namespace echo
{
    public class Server
    {
        private TcpClient client;
        private IPAddress ip = Dns.GetHostEntry("localhost").AddressList[0];
        private TcpListener server;

        public void ServerSetup()
        {
            server = new TcpListener(ip, 123);
            try
            {
                server.Start();
            }
            catch (Exception exception)
            {
                throw new Exception(exception.Message);
            }

            while (true)
            {
                client = server.AcceptTcpClient();
                HandleClient cl = new HandleClient(); //server sends a message to a new connected client
                cl.StartClient(client);
            }
        }
    }
}