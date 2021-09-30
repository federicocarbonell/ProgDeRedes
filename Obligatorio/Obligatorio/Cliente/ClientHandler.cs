using Client.Interfaces;
using ProtocolLibrary;
using System;
using System.Collections.Generic;
using System.IO;
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
                throw new Exception($"No se pudo conectar al servidor, {e.Message}");
            }
        }

        public void AddGame(string title, string genre, string trailer, string cover)
        {
            
            List<byte> data = new List<byte>();

            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);

            SendData(data, CommandConstants.AddGame);
            
            try
            {
                SendFileData(cover, title);
            }
            catch(Exception e)
            {
                Console.WriteLine("No se pudo enviar el archivo de portada");
            }
            
            Recieve();
        }

        public void ViewBoughtGames(string username)
        {
            List<byte> data = new List<byte>();

            AddStringData(data, username);

            SendData(data, CommandConstants.ViewBoughtGames);
            Recieve();
        }

        public void BuyGame(string username, int id)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddStringData(data, username);

            SendData(data, CommandConstants.BuyGame);
            Recieve();
        }

        public void DeleteGame(int id)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            SendData(data, CommandConstants.DeleteGame);
            Recieve();
        }

        public void ModifyGame(int id, string title, string genre, string trailer, string cover)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);

            SendData(data, CommandConstants.ModifyGame);
            try
            {
                SendFileData(cover, title);
            }
            catch(Exception e)
            {
                Console.WriteLine("No se pudo enviar el archivo de portada");
            }
            Recieve();
        }

        public void QualifyGame(int id, int rating, string review)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddIntData(data, rating);
            AddStringData(data, review);

            SendData(data, CommandConstants.QualifyGame);
            Recieve();
        }

        public void ViewGameDetail(int id)
        {

            List<byte> data = new List<byte>();

            AddIntData(data, id);

            SendData(data, CommandConstants.ViewDetail);
            Recieve();
        }

        public void ViewGames()
        {

            List<byte> data = new List<byte>();

            SendData(data, CommandConstants.GetGames);
            Recieve();
        }

        public void SearchForGames(string searchMode, string searchTerm, string minRating)
        {
            List<byte> data = new List<byte>();
            int mode = Int32.Parse(searchMode);
            if (string.IsNullOrEmpty(minRating)) minRating = "0";
            
            AddIntData(data, mode);
            AddStringData(data, searchTerm);
            AddIntData(data, Int32.Parse(minRating));
            

            SendData(data, CommandConstants.SearchForGame);
            Recieve();
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

        private void SendFileData(string path, string gameName)
        {
            //envio nombre y largo de la imagen
            List<byte> req = new List<byte>();
            FileInfo coverInfo = new FileInfo(path);
            long fileSize = coverInfo.Length;
            string ext = coverInfo.Extension;

            AddStringData(req, gameName + ext);
            AddLongData(req, fileSize);
            SendData(req, CommandConstants.SendGameCover);
            //envio nombre y largo de la imagen

            //envio la imagen
            long fileParts = FileTransferProtocol.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = FileStreamHandler.ReadData(path, FileTransferProtocol.MaxPacketSize, offset);
                    offset += FileTransferProtocol.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = FileStreamHandler.ReadData(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                int auxOffset = 0;
                int auxSize = data.Length;
                while(auxOffset < data.Length)
                {
                    int sent = socket.Send(data, auxOffset, auxSize - auxOffset, SocketFlags.None);
                    if(sent == 0)
                    {
                        throw new SocketException();
                    }
                    auxOffset += sent;
                }
                currentPart++;
            }
        }

        private void AddIntData(List<byte> data, int info)
        {
            byte[] bytes = BitConverter.GetBytes(info);
            data.AddRange(BitConverter.GetBytes(bytes.Length));
            data.AddRange(bytes);
        }

        private void AddLongData(List<byte> data, long info)
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

        private void Recieve()
        {

            byte[] bytes = new byte[1024];

            while (true)
            {
                int bytesRec = socket.Receive(bytes);
                var data = Encoding.UTF8.GetString(bytes, 0, bytesRec);

                if (data.IndexOf("<EOF>") > -1)
                {
                    Console.WriteLine("Texto recibido : \n {0}", data.Replace("<EOF>", ""));
                    break;
                }
            }

        }

        internal void Logout()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

    }
}
