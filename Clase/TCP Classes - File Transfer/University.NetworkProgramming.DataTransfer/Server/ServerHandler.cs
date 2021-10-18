using System.Net;
using System.Net.Sockets;
using Common;

namespace Server
{
    public class ServerHandler
    {
        private readonly TcpListener _tcpListener;
        private readonly IPEndPoint _serverIpEndPoint;

        public ServerHandler()
        {
            _serverIpEndPoint = new IPEndPoint(IPAddress.Loopback, 7000);
            _tcpListener = new TcpListener(_serverIpEndPoint);
            _tcpListener.Start(1);
        }

        public void ReceiveFile()
        {
            TcpClient tcpClient = _tcpListener.AcceptTcpClient();
            _tcpListener.Stop();
            var fileCommunication = new FileCommunicationHandler(tcpClient);
            fileCommunication.ReceiveFile();
        }
    }
}