using Client.DTOs;
using Client.Interfaces;
using ProtocolLibrary;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    public class ClientHandler : IClientHandler
    {

        private const string ServerIp = "127.0.0.1";
        private const string ClientIp = "127.0.0.1";
        private const int ServerPort = 20000;
        private const int ClientPort = 0;
        public readonly Socket socket;
        private Header header;
        //lei en internet de thread pools o socket pools

        public ClientHandler()
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(new IPEndPoint(IPAddress.Parse(ClientIp), ClientPort));
            ConnectToServer();
        }

        public void ConnectToServer()
        {
            try
            {
                socket.Connect(ServerIp, ServerPort);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Could not connect to server, {e.Message}");
            }
        }

        public void AddGame(string title, string genre, string trailer, string cover)
        {
            
            List<byte> data = new List<byte>();

            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);
            AddStringData(data, cover);

            SendData(data, CommandConstants.AddGame);

        }

        public void DeleteGame(int id)
        {
            //creo q no tamo usando el result para nada habria q revisar bien
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            SendData(data, CommandConstants.DeleteGame);

        }

        public void ModifyGame(int id, string title, string genre, string trailer, string cover)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);
            AddStringData(data, cover);

            SendData(data, CommandConstants.ModifyGame);

        }

        public void QualifyGame(int id, int rating, string review)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddIntData(data, rating);
            AddStringData(data, review);

            SendData(data, CommandConstants.QualifyGame);

        }

        public void ViewGameDetail(int id)
        {

            List<byte> data = new List<byte>();

            AddIntData(data, id);

            SendData(data, CommandConstants.ViewDetail);

        }

        public void ViewGames()
        {

            List<byte> data = new List<byte>();

            SendData(data, CommandConstants.GetGames);

        }

        private void SendData(List<byte> data, int command)
        {

            header = new Header(HeaderConstants.Request, command, data.Count);

            byte[] headerBytes = header.GetRequest();

            int sentHeaderBytes = 0;
            while (sentHeaderBytes < headerBytes.Length)
            {
                sentHeaderBytes += socket.Send(headerBytes, sentHeaderBytes, headerBytes.Length - sentHeaderBytes, SocketFlags.None);
            }

            if(data.Count != 0)
            {
                int sentBodyBytes = 0;
                while (sentBodyBytes < data.Count)
                {
                    sentBodyBytes += socket.Send(data.ToArray(), sentBodyBytes, data.Count - sentBodyBytes, SocketFlags.None);
                }
            }

        }

        private void AddIntData(List<byte> data, int info)
        {
            byte[] bytes = BitConverter.GetBytes(info);
            data.AddRange(BitConverter.GetBytes(bytes.Length));
            data.AddRange(bytes);
        }

        private void AddStringData(List<byte> data, string info)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(info);
            data.AddRange(BitConverter.GetBytes(bytes.Length));
            data.AddRange(bytes);
        }

    }
}
