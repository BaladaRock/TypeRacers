﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;


namespace TypeRacers.Client
{
    public class TypeRacersClient
    {
        TcpClient client;
        NetworkStream stream;
        public List<string> Opponents { get; set; }
        public string Name { get; set; }
   
        public void SendProgressToServer(string progress)
        {
            //connecting to server
            client = new TcpClient("localhost", 80);
            stream = client.GetStream();
            //writing the progress to stream
            byte[] bytesToSend = Encoding.ASCII.GetBytes(progress + "$" + Name + "#");
            stream.Write(bytesToSend, 0, bytesToSend.Length);
            GetOpponentsProgress();
            stream.Flush();
        }

        private void GetOpponentsProgress()
        {
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
                client.Close();
                Opponents = text.Split('/').ToList();
                Opponents.Remove("#");
            }
            catch (Exception)
            {
                throw new Exception("Lost connection with server");
            }
        }

        public string GetMessageFromServer()
        {
            client = new TcpClient("localhost", 80);
            stream = client.GetStream();
            try
            {
                byte[] bytesToSend = Encoding.ASCII.GetBytes("0" + "$" + Name + "#");
                stream.Write(bytesToSend, 0, bytesToSend.Length);

                byte[] inStream = new byte[client.ReceiveBufferSize];
                int read = stream.Read(inStream, 0, inStream.Length);
                string text = Encoding.ASCII.GetString(inStream, 0, read);
                while (!text[read - 1].Equals('#'))
                {
                    read = stream.Read(inStream, 0, inStream.Length);
                    text += Encoding.ASCII.GetString(inStream, text.Length, read);
                }
                client.Close();
                return text.Substring(0, text.IndexOf('#'));
            }
            catch (Exception)
            {
                throw new Exception("Lost connection with server");
            }
        }
    }
}
