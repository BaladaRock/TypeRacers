using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TypeRacers.Server
{
    class ServerSetup
    {
        private bool newClient;
        private TcpListener server;
        private Dictionary<string, Tuple<string, string>> players;
        private NetworkStream networkStream;
        private string currentClient;
        //to avoid generating different texts from users in same competition
        public static string CompetitionText { get; } = ServerGeneratedText.GetText();

        public void Setup()
        {
            server = new TcpListener(IPAddress.IPv6Any, 80);
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            players = new Dictionary<string, Tuple<string, string>>();

            try
            {
                server.Start();
            }
            catch (Exception)
            {
                throw new Exception("Server setup failed");
            }

            Console.WriteLine("Server started");
            CommunicationSetup();//separated the communication from server starter
        }

        private void CommunicationSetup()
        {
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();

                try
                {
                    //creates the stream
                    networkStream = client.GetStream();
                    //reads from stream
                    byte[] buffer = new byte[client.ReceiveBufferSize];
                    int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                    string dataReceived = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    //solution to get get complete messages
                    while (!dataReceived[bytesRead - 1].Equals('#'))
                    {
                        bytesRead = networkStream.Read(buffer, 0, client.ReceiveBufferSize);
                        dataReceived += Encoding.ASCII.GetString(buffer, dataReceived.Length, bytesRead);
                    }

                    CheckUsername(dataReceived, players);
                    //check if reading from the stream has been done on the other end in order to close client
                    if (networkStream.DataAvailable)
                    { 
                        client.Close(); 
                    }
                    Console.WriteLine("info: " + dataReceived);
                    Console.WriteLine("Disconnected client");
                }
                catch (Exception)
                {
                    Console.WriteLine("Lost connection with client");
                }
            }
        }

        private void CheckNewClient()
        {
            if (newClient)
            {
                byte[] broadcastBytes = Encoding.ASCII.GetBytes(CompetitionText + "#"); //generates random text from text document
                networkStream.Write(broadcastBytes, 0, broadcastBytes.Length);//send the text to connected client
                //SendOpponents();//sending current connected opponents to connected client
            }
            else
            {
                SendOpponents();
            }
        }

        private void SendOpponents()
        {
            string opponents = string.Empty;
            foreach (var a in players)
            {
                if (!a.Key.ToString().Equals(currentClient))
                {
                    opponents += a.Key + ":" + a.Value.Item1 + "&" + a.Value.Item2 + "/";
                }
            }

            byte[] broadcastBytes = Encoding.ASCII.GetBytes(opponents + "#");
            networkStream.Write(broadcastBytes, 0, broadcastBytes.Length);
        }


        //this method determines if a player is new or is already playing and is just sending progress
        private void CheckUsername(string dataReceived, Dictionary<string, Tuple<string, string>> players)
        {
            string progressAndSliderProgress = dataReceived.Substring(0, dataReceived.IndexOf('$'));
            Tuple<string, string> clientInfo = new Tuple<string, string>(progressAndSliderProgress.Substring(0, progressAndSliderProgress.IndexOf('&')), progressAndSliderProgress.Substring(progressAndSliderProgress.IndexOf('&') + 1));
            string username = dataReceived.Substring(dataReceived.IndexOf('$') + 1);
            currentClient = username.Substring(0, username.Length - 1);
            Console.WriteLine(username + " connected");

            if (players.ContainsKey(currentClient))
            {
                newClient = false;
                players[currentClient] = clientInfo;
            }
            else
            {
                newClient = true;
                players.Add(currentClient, clientInfo);
            }

            CheckNewClient();
        }
    }
}

