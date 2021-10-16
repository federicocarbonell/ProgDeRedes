using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
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

        public async Task ReceiveFileAsync()
        {
            TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync();
            _tcpListener.Stop();
            var fileCommunication = new FileCommunicationHandler(tcpClient);
            await fileCommunication.ReceiveFileAsync();
        }
    }
}