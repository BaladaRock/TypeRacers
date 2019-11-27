﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class Client
    {
        readonly TcpClient client = new TcpClient("localhost", 123);
        private NetworkStream stream;

        //returns a string to connect to the MVVM
        public string GetMessageFromServer()
        {
            stream = client.GetStream();
            try
            {
                byte[] inStream = new byte[10025];
                var read = stream.Read(inStream, 0, inStream.Length);
                var data = Encoding.ASCII.GetString(inStream, 0, read);
                return data;
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
}