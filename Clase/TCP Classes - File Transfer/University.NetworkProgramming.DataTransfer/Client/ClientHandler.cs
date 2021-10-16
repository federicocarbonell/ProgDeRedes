using System.Net;
using System.Net.Sockets;
using Common;

namespace Client
{
    public class ClientHandler
    {
        private readonly TcpClient _tcpClient;
        private readonly IPEndPoint _clientIpEndPoint;
        private readonly IPEndPoint _serverIpEndPoint;

        public ClientHandler()
        {
            _clientIpEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            _tcpClient = new TcpClient(_clientIpEndPoint);
            _serverIpEndPoint = new IPEndPoint(IPAddress.Loopback, 7000);
        }

        public void SendFile(string path)
        {
            _tcpClient.Connect(_serverIpEndPoint);
            var fileCommunication = new FileCommunicationHandler(_tcpClient);
            fileCommunication.SendFile(path);
        }

        public void CloseConnection()
        {
            _tcpClient.GetStream().Close();
            _tcpClient.Close();
        }
    }
}