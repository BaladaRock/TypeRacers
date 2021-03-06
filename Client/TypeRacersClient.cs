﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Timers;

namespace TypeRacers.Client
{
    public class TypeRacersClient
    {
        TcpClient client;
        NetworkStream stream;
        Timer timer;
        readonly int interval = 1000; // 1 second
        readonly int totalTime = 30000; // 15 seconds or 15000 ms
        int elapsedTime = 0; // Elapsed time in ms
        List<Tuple<string, Tuple<string, string, int>>> opponents;



        public delegate void TimerTickHandler(Tuple<List<Tuple<string, Tuple<string, string, int>>>, int> opponentsAndElapsedTime);
        public event TimerTickHandler OpponentsChanged;

        private void SetOpponentsAndElapsedTime(Tuple<List<Tuple<string, Tuple<string, string, int>>>, int> value)

        {
            opponents = value.Item1;
            elapsedTime = value.Item2;
            OnOpponentsChangedAndTimeChanged(value);
        }

        public string StartingTime { get; set; }
        private string LocalPlayerProgress { get; set; }
        private int PlayroomNumber { get; set; }
        public static string Name { get; set; }
        public void StartTimerForSearchingOpponents()
        {
            timer = new Timer(interval);
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Enabled = true;
        }

        public void NameClient(string username)
        {
            Name = username;
        }
        void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            timer.Stop();
            if (elapsedTime > totalTime)
            {
                elapsedTime = 0;
                //we stop the timer after 30 seconds
                return;
            }
            else
            {
                // here I am performing the task
                //getting the opponents each second for 30 seconds from server through Client

                SetOpponentsAndElapsedTime(new Tuple<List<Tuple<string, Tuple<string, string, int>>>, int>(GetOpponentsProgress(), elapsedTime));

                timer.Enabled = true;
            }
            elapsedTime += interval;
        }

        protected void OnOpponentsChangedAndTimeChanged(Tuple<List<Tuple<string, Tuple<string, string, int>>>, int> opponentsAndElapsedTime)

        {
            if (opponentsAndElapsedTime != null)
            {
                OpponentsChanged(opponentsAndElapsedTime);
            }
        }
        public void SendProgressToServer(string progress)
        {
            //connecting to server
            client = new TcpClient("localhost", 80);
            stream = client.GetStream();

            //getting the current progress from versus view model
            LocalPlayerProgress = progress;
            //writing the progress to stream
            byte[] bytesToSend = Encoding.ASCII.GetBytes(LocalPlayerProgress + "&" + PlayroomNumber + "$" + Name + "#");
            stream.Write(bytesToSend, 0, bytesToSend.Length);

            SetOpponentsAndElapsedTime(new Tuple<List<Tuple<string, Tuple<string, string, int>>>, int>(GetOpponentsProgress(), elapsedTime));

            stream.Flush();
        }
        //receiving the opponents and their progress in a List

        public List<Tuple<string, Tuple<string, string, int>>> GetOpponentsProgress()

        {
            //connecting to server
            client = new TcpClient("localhost", 80);
            stream = client.GetStream();
            string toSend;


            //writing the progress to stream
            if (LocalPlayerProgress != null)
            {
                toSend = LocalPlayerProgress + "&" + PlayroomNumber + "$" + Name + "#";
            }
            else
            {
                toSend = "0&0&" + PlayroomNumber + "$" + Name + "#";

            }
            byte[] bytesToSend = Encoding.ASCII.GetBytes(toSend);
            stream.Write(bytesToSend, 0, bytesToSend.Length);

            try
            {
                byte[] inStream = new byte[client.ReceiveBufferSize];
                int read = stream.Read(inStream, 0, inStream.Length);
                string text = Encoding.ASCII.GetString(inStream, 0, read);

                while (!text[read - 1].Equals('#'))
                {
                    read = stream.Read(inStream, 0, inStream.Length);
                    text += Encoding.ASCII.GetString(inStream, text.Length, read);
                }

                var currentOpponents = text.Split('/').ToList();
                currentOpponents.Remove("#");

                opponents = new List<Tuple<string, Tuple<string, string, int>>>();
                foreach (var v in currentOpponents)
                {
                    var nameAndInfos = v.Split(':');

                    var progressInfoAndPlayroomNumber = nameAndInfos.LastOrDefault().Split('&');

                    var opponentInfo = new Tuple<string, string, int>(progressInfoAndPlayroomNumber[0], progressInfoAndPlayroomNumber[1],
                        Convert.ToInt32(progressInfoAndPlayroomNumber[2]));

                    opponents.Add(new Tuple<string, Tuple<string, string, int>>(nameAndInfos.FirstOrDefault(), opponentInfo));
                }

                return opponents;
            }
            catch (Exception)
            {
                throw new Exception("Lost connection with server");
            }
        }
        public string GetMessageFromServer()
        {
            //connecting to server
            client = new TcpClient("localhost", 80);
            stream = client.GetStream();

            try
            {
                byte[] bytesToSend = Encoding.ASCII.GetBytes("0&0&" + PlayroomNumber + "$" + Name + "#");

                stream.Write(bytesToSend, 0, bytesToSend.Length);

                byte[] inStream = new byte[client.ReceiveBufferSize];
                int read = stream.Read(inStream, 0, inStream.Length);
                string recievedData = Encoding.ASCII.GetString(inStream, 0, read);

                while (!recievedData[read - 1].Equals('#'))
                {
                    read = stream.Read(inStream, 0, inStream.Length);
                    recievedData += Encoding.ASCII.GetString(inStream, recievedData.Length, read);
                }

                var dataWithoutHashtag = recievedData.Remove(recievedData.Length - 1);
                var textToType = dataWithoutHashtag.Substring(0, dataWithoutHashtag.IndexOf('$'));
                //getting the starting time
                StartingTime = dataWithoutHashtag.Substring(dataWithoutHashtag.IndexOf('%') + 1);
                //getting room number
                PlayroomNumber = Convert.ToInt32(dataWithoutHashtag.Substring(dataWithoutHashtag.IndexOf('$') + 1, (dataWithoutHashtag.Length - textToType.Length - StartingTime.Length - 2)));



                client.Close();
                return textToType;
            }
            catch (Exception)
            {
                throw new Exception("Lost connection with server");
            }
        }
    }
}
