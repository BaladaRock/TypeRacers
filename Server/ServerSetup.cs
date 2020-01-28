using Server;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TypeRacers.Server
{
    class ServerSetup
    {
        private bool newClient;
        private TcpListener server;
        private NetworkStream networkStream;
        private string currentClient;
        private List<Playroom> playrooms;
        private Playroom currentPlayroom;
        int playroomCount = 0;
        int currentPlayerPlayroomNumber;
        int maxPlayroomSize = 3;


        //to avoid generating different texts from users in same competition


        public static string CompetitionText { get; } = ServerGeneratedText.GetText();
        public void Setup()
        {

            server = new TcpListener(IPAddress.IPv6Any, 80);
            server.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
            playrooms = new List<Playroom>();
            playrooms.Add(new Playroom());
            currentPlayroom = playrooms.Last();


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


                    CheckClientReceievedData(dataReceived);
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

        private void CheckNewClient(int roomNumber)
        {
            if (newClient)
            {
                byte[] broadcastBytes = Encoding.ASCII.GetBytes(CompetitionText + "$" + roomNumber + "%" + currentPlayroom.StartingTime + "#"); //generates random text from text document
                networkStream.Write(broadcastBytes, 0, broadcastBytes.Length);//send the text to connected client

            }
            else
            {
                SendOpponents();
            }
        }

        private void SendOpponents()
        {
            string opponents = string.Empty;

            foreach (var a in playrooms[currentPlayerPlayroomNumber].Players)
            {
                if (!a.Key.ToString().Equals(currentClient))
                {
                    opponents += a.Key + ":" + a.Value.Item1 + "&" + a.Value.Item2 + "&" + a.Value.Item3 + "/";

                }
            }

            byte[] broadcastBytes = Encoding.ASCII.GetBytes(opponents + "#");
            networkStream.Write(broadcastBytes, 0, broadcastBytes.Length);
        }

        //this method determines if a player is new or is already playing and is just sending progress

        private void CheckClientReceievedData(string dataReceived)
        {

            //progress and slider progress
            string progress = dataReceived.Substring(0, dataReceived.IndexOf('$'));

            var progressInfoAndPlayerRoom = progress.Split('&');

            //1 progress, 2 sliderprogress, 3 current client playroom
            Tuple<string, string, int> clientInfo = new Tuple<string, string, int>(progressInfoAndPlayerRoom[0],
                progressInfoAndPlayerRoom[1], Convert.ToInt32(progressInfoAndPlayerRoom[2]));


            string username = dataReceived.Substring(dataReceived.IndexOf('$') + 1);
            currentClient = username.Substring(0, username.Length - 1);

            if (currentClient == string.Empty)
            {
                return;
            }

            currentPlayerPlayroomNumber = clientInfo.Item3;

            CheckCurrentPlayroom(currentClient, currentPlayerPlayroomNumber, clientInfo);

        }

        public Playroom CreateNewPlayroom()
        {
            playroomCount++;
            var newPlayroom = new Playroom();
            newPlayroom.PlayroomNumber = playroomCount;
            playrooms.Add(newPlayroom);
            return playrooms.Last();
        }

        public void CheckCurrentPlayroom(string currrentClient, int roomNumber, Tuple<string, string, int> clientInfo)
        {
            currentPlayroom = playrooms[roomNumber];


            if (currentPlayroom.PlayroomSize == maxPlayroomSize && !currentPlayroom.ExistsInPlayroom(currentClient))
            {
                if (playrooms.Last().PlayroomSize == maxPlayroomSize)
                {

                    currentPlayroom = CreateNewPlayroom();
                }
                else
                {

                    currentPlayroom = playrooms.Last();
                }

            }
            newClient = currentPlayroom.AddPlayersToRoom(currrentClient, clientInfo);
            CheckNewClient(currentPlayroom.PlayroomNumber);

        }
    }
}
