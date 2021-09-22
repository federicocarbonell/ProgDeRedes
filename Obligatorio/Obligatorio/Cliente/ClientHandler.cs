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
            
            GameDTO game = new GameDTO { Name = title, Genre = genre, Description = trailer, CoverPath = cover };

            List<byte> result = new List<byte>();
            List<byte> data = new List<byte>();

            //pa esto no podremos hacer un strategy re picante
                //le pasas el objeto, setea la strategy
                //en funcion de la strategy hace la conversion, dejamos en una clase estatica las auxiliares (int/string/algo -> byte[])

            //ARMO TRAMA DE DATOS
            //paso a bytes info a pasar
            byte[] titleBytes = Encoding.UTF8.GetBytes(title);
            //agrego tamaño de info a pasar
            data.AddRange(BitConverter.GetBytes(titleBytes.Length));
            //agrego info a pasar
            data.AddRange(titleBytes);

            //paso a bytes info a pasar
            byte[] genreBytes = Encoding.UTF8.GetBytes(genre);
            //agrego tamaño de info a pasar
            data.AddRange(BitConverter.GetBytes(genreBytes.Length));
            //agrego info a pasar
            data.AddRange(genreBytes);

            //paso a bytes info a pasar
            byte[] trailerBytes = Encoding.UTF8.GetBytes(trailer);
            //agrego tamaño de info a pasar
            data.AddRange(BitConverter.GetBytes(trailerBytes.Length));
            //agrego info a pasar
            data.AddRange(trailerBytes);

            //paso a bytes info a pasar
            byte[] coverBytes = Encoding.UTF8.GetBytes(cover);
            //agrego tamaño de info a pasar
            data.AddRange(BitConverter.GetBytes(coverBytes.Length));
            //agrego info a pasar
            data.AddRange(coverBytes);
            //TERMINO ARMADO TRAMA DATOS

            //ARRANCO A ARMAR ENCABEZADO
            //seteo comando
            result.AddRange(BitConverter.GetBytes(CommandConstants.AddGame));
            //seteo largo datos a enviar
            result.AddRange(BitConverter.GetBytes(data.Count));
            //seteo datos
            result.AddRange(data);
            //TERMINO DE ARMAR EL ENCABEZADO

            header = new Header(HeaderConstants.Request, CommandConstants.AddGame, data.Count);

            byte[] headerBytes = header.GetRequest();

            int sentHeaderBytes = 0;
            while(sentHeaderBytes < headerBytes.Length)
            {
                sentHeaderBytes += socket.Send(headerBytes, sentHeaderBytes, headerBytes.Length - sentHeaderBytes, SocketFlags.None);
            }

            int sentBodyBytes = 0;
            while(sentBodyBytes < data.Count)
            {
                sentBodyBytes += socket.Send(data.ToArray(), sentBodyBytes, data.Count - sentBodyBytes, SocketFlags.None);
            }
        }

        public void DeleteGame(int id)
        {
            //creo q no tamo usando el result para nada habria q revisar bien
            List<byte> result = new List<byte>();
            List<byte> data = new List<byte>();

            byte[] gameIdBytes = BitConverter.GetBytes(id);
            data.AddRange(BitConverter.GetBytes(gameIdBytes.Length));
            data.AddRange(gameIdBytes);

            //result.AddRange(CommandConstants.DeleteGame);
            result.AddRange(BitConverter.GetBytes(data.Count));
            result.AddRange(data);

            header = new Header(HeaderConstants.Request, CommandConstants.DeleteGame, data.Count);

            byte[] headerBytes = header.GetRequest();

            int sentHeaderBytes = 0;
            while (sentHeaderBytes < headerBytes.Length)
            {
                sentHeaderBytes += socket.Send(headerBytes, sentHeaderBytes, headerBytes.Length - sentHeaderBytes, SocketFlags.None);
            }

            int sentBodyBytes = 0;
            while (sentBodyBytes < data.Count)
            {
                sentBodyBytes += socket.Send(data.ToArray(), sentBodyBytes, data.Count - sentBodyBytes, SocketFlags.None);
            }
        }

        public void ModifyGame(int id, string title, string genre, string trailer, string cover)
        {
            throw new NotImplementedException();
        }

        public void QualifyGame(int id, int rating, string review)
        {
            throw new NotImplementedException();
        }

        public void ViewGameDetail(int id)
        {
            throw new NotImplementedException();
        }

        public void ViewGames()
        {
            throw new NotImplementedException();
        }
    }
}
