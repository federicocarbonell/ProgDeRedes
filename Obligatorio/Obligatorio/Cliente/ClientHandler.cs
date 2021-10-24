using Client.Interfaces;
using ProtocolLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Common;

namespace Client
{
    public class ClientHandler //: IClientHandler
    {

        private static string ServerIp;
        private static string ClientIp;
        private static int ServerPort;
        private static int ClientPort;
        public readonly Socket socket;
        private static TcpClient tcpClient;
        private static IPEndPoint clientIpEndPoint;
        private Header header;

        public ClientHandler(IConfiguration configuration)
        {
            ObtainConfigParameters(configuration);

            clientIpEndPoint = new IPEndPoint(IPAddress.Parse(ClientIp), ClientPort);
            tcpClient = new TcpClient(clientIpEndPoint);
           
            ConnectToServer();
        }
        
        public void ConnectToServer()
        {
            try
            {
                tcpClient.ConnectAsync(IPAddress.Parse(ServerIp), ServerPort);
            }
            catch (Exception e)
            {
                throw new Exception($"No se pudo conectar al servidor, {e.Message}");
            }
        }
        
        private static void ObtainConfigParameters(IConfiguration configuration)
        {
            ServerIp = configuration["ServerIp"];
            ClientIp = configuration["ClientIp"];
            ServerPort = Int32.Parse(configuration["ServerPort"]);
            ClientPort = Int32.Parse(configuration["ClientPort"]);
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            List<byte> data = new List<byte>();

            AddStringData(data, username);
            AddStringData(data, password);

            await SendDataAsync(data, CommandConstants.Login);

            return ReceiveLogin();
        }

        public async Task AddGameAsync(string title, string genre, string trailer, string cover)
        {
            
            List<byte> data = new List<byte>();

            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);

            await SendDataAsync(data, CommandConstants.AddGame);

            try
            {
                await SendFileDataAsync(cover, title);
            }
            catch (Exception e)
            {
                Console.WriteLine("No se pudo enviar el archivo de portada");
            }

            Recieve();
        }

        public async Task ViewBoughtGamesAsync()
        {
            List<byte> data = new List<byte>();
            await SendDataAsync(data, CommandConstants.ViewBoughtGames);
            Recieve();
        }

        public async Task BuyGameAsync(int id)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);

            await SendDataAsync(data, CommandConstants.BuyGame);
            Recieve();
        }

        public async Task DeleteGameAsync(int id)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            await SendDataAsync(data, CommandConstants.DeleteGame);
            Recieve();
        }

        public async Task ModifyGameAsync(int id, string title, string genre, string trailer, string cover)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddStringData(data, title);
            AddStringData(data, genre);
            AddStringData(data, trailer);

            await SendDataAsync(data, CommandConstants.ModifyGame);
            try
            {
                await SendFileDataAsync(cover, title);
            }
            catch(Exception e)
            {
                Console.WriteLine("No se pudo enviar el archivo de portada");
            }
            Recieve();
        }

        public async Task QualifyGameAsync(int id, int rating, string review)
        {
            List<byte> data = new List<byte>();

            AddIntData(data, id);
            AddIntData(data, rating);
            AddStringData(data, review);

            await SendDataAsync(data, CommandConstants.QualifyGame);
            Recieve();
        }

        public async Task ViewGameDetailAsync(int id)
        {

            List<byte> data = new List<byte>();

            AddIntData(data, id);

            await SendDataAsync(data, CommandConstants.ViewDetail);
            Recieve();
        }

        public async Task ViewGamesAsync()
        {

            List<byte> data = new List<byte>();

            await SendDataAsync(data, CommandConstants.GetGames);
            Recieve();
        }

        public async Task SearchForGamesAsync(string searchMode, string searchTerm, string minRating)
        {
            List<byte> data = new List<byte>();
            int mode = Int32.Parse(searchMode);
            if (string.IsNullOrEmpty(minRating)) minRating = "0";
            
            AddIntData(data, mode);
            AddStringData(data, searchTerm);
            AddIntData(data, Int32.Parse(minRating));
            

            await SendDataAsync(data, CommandConstants.SearchForGame);
            Recieve();
        }

        private async Task SendDataAsync(List<byte> data, int command)
        {

            header = new Header(HeaderConstants.Request, command, data.Count);

            byte[] headerBytes = header.GetRequest();

            int sentHeaderBytes = 0;
            
            await tcpClient.GetStream().WriteAsync(headerBytes, sentHeaderBytes, headerBytes.Length - sentHeaderBytes, CancellationToken.None);

            if(data.Count != 0)
            {
                int sentBodyBytes = 0;
                await tcpClient.GetStream().WriteAsync(data.ToArray(), sentBodyBytes, data.Count - sentBodyBytes, CancellationToken.None);
            }

        }

        private async Task SendFileDataAsync(string path, string gameName)
        {
            //envio nombre y largo de la imagen
            List<byte> req = new List<byte>();
            FileInfo coverInfo = new FileInfo(path);
            long fileSize = coverInfo.Length;
            string ext = coverInfo.Extension;

            AddStringData(req, gameName + ext);
            AddLongData(req, fileSize);
            await SendDataAsync(req, CommandConstants.SendGameCover);
            //envio nombre y largo de la imagen

            //envio la imagen
            var fileCommunication = new FileCommunicationHandler(tcpClient);
            await fileCommunication.SendFileAsync(path);
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
                //int bytesRec = socket.Receive(bytes);
                int bytesRect = tcpClient.GetStream().Read(bytes);
                var data = Encoding.UTF8.GetString(bytes, 0, bytesRect);
                Console.WriteLine("Texto recibido : \n {0}", data); 

                if (data.IndexOf("<EOF>") > -1)
                {
                    break;
                }
            }

        }

        private bool ReceiveLogin()
        {
            byte[] bytes = new byte[1024];

            while (true)
            {
                int bytesRect = tcpClient.GetStream().Read(bytes);
                var data = Encoding.UTF8.GetString(bytes, 0, bytesRect);

                if (data.IndexOf("TokenAuth") > -1)
                {
                    return true;
                }

                return false;
            }
        }

        internal void Logout()
        {
            tcpClient.Close();
            //socket.Shutdown(SocketShutdown.Both);
            //socket.Close();
        }

    }
}
