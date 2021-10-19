using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ProtocolLibrary
{
    public class NetworkStreamHandler
    {
        private readonly TcpClient client;

        public NetworkStreamHandler(TcpClient client)
        {
            this.client = client;
        }

        public async Task<byte[]> ReceiveDataAsync(int length)
        {
            int offset = 0;
            byte[] response = new byte[length];
            while (offset < length)
            {
                int received = await client.Client.ReceiveAsync(response, SocketFlags.None);
                if (received == 0)
                {
                    throw new SocketException();
                }

                offset += received;
            }
            return response;
        }

        public async Task SendDataAsync(byte[] data)
        {
            int offset = 0;
            int size = data.Length;
            while (offset < data.Length)
            {
                int sent = await client.Client.SendAsync(data, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                offset += sent;
            }
        }
    }
}
