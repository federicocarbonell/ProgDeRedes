using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common;

namespace Client
{
    public class ClientHandler
    {
        private readonly TcpClient _tcpClient;
        private readonly IPEndPoint _clientIpEndPoint;

        public ClientHandler()
        {
            _clientIpEndPoint = new IPEndPoint(IPAddress.Loopback, 0);
            _tcpClient = new TcpClient(_clientIpEndPoint);
        }

        public async Task SendFileAsync(string path)
        {
            await _tcpClient.ConnectAsync(IPAddress.Loopback, 7000);
            var fileCommunication = new FileCommunicationHandler(_tcpClient);
            await fileCommunication.SendFileAsync(path);
        }

        public void CloseConnection()
        {
            _tcpClient.GetStream().Close();
            _tcpClient.Close();
        }
    }
}