using System;
using System.Net;
using System.Net.Sockets;

namespace ProtocolLibrary
{
    public class SocketStreamHandler
    {
        private readonly Socket _socket;

        public SocketStreamHandler(Socket socket)
        {
            _socket = socket;
        }

        public byte[] ReceiveData(Socket clientSocket, int length)
        {
            int offset = 0;
            byte[] response = new byte[length];
            while (offset < length)
            {
                int received = clientSocket.Receive(response, offset, length - offset, SocketFlags.None);
                if (received == 0)
                {
                    throw new SocketException();
                }

                offset += received;
            }
            return response;
        }

        public void SendData(byte[] data)
        {
            int offset = 0;
            int size = data.Length;
            while (offset < data.Length)
            {
                int sent = _socket.Send(data, offset, size - offset, SocketFlags.None);
                if (sent == 0)
                {
                    throw new SocketException();
                }
                offset += sent;
            }
        }
    }
}
