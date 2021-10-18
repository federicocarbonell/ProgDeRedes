using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Common
{
    public class FileCommunicationHandler
    {
        private readonly FileStreamHandler _fileStreamHandler;
        private readonly NetworkStreamHandler _networkStreamHandler;

        public FileCommunicationHandler(TcpClient tcpClient)
        {
            _networkStreamHandler = new NetworkStreamHandler(tcpClient.GetStream());
            _fileStreamHandler = new FileStreamHandler();
        }

        public void SendFile(string path)
        {
            var fileInfo = new FileInfo(path);
            string fileName = fileInfo.Name;
            byte[] fileNameData = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameData.Length;
            byte[] fileNameLengthData = BitConverter.GetBytes(fileNameLength);
            // 1.- Envío el largo del nombre del archivo
            _networkStreamHandler.SendData(fileNameLengthData);
            // 2.- Envío el nombre del archivo
            _networkStreamHandler.SendData(fileNameData);

            long fileSize = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileSize);
            // 3.- Envío el largo del archivo
            _networkStreamHandler.SendData(fileSizeDataLength);
            // 4.- Envío los datos del archivo
            SendFile(fileSize, path);
        }

        public void ReceiveFile()
        {
            // 1.- Recibir el largo del nombre del archivo
            byte[] fileNameLengthData = _networkStreamHandler.ReadData(ProtocolSpecification.FileNameSize);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);
            // 2.- Recibir el nombre del archivo
            byte[] fileNameData = _networkStreamHandler.ReadData(fileNameLength);
            string fileName = Encoding.UTF8.GetString(fileNameData);

            // 3.- Recibo el largo del archivo
            byte[] fileSizeDataLength = _networkStreamHandler.ReadData(ProtocolSpecification.FileSize);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);
            ReceiveFile(fileSize, fileName);
        }

        private void SendFile(long fileSize, string path)
        {
            long fileParts = ProtocolSpecification.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = _fileStreamHandler.ReadData(path, ProtocolSpecification.MaxPacketSize, offset);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = _fileStreamHandler.ReadData(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                _networkStreamHandler.SendData(data);
                currentPart++;
            }
        }

        private void ReceiveFile(long fileSize, string fileName)
        {
            long fileParts = ProtocolSpecification.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = _networkStreamHandler.ReadData(ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = _networkStreamHandler.ReadData(lastPartSize);
                    offset += lastPartSize;
                }
                _fileStreamHandler.WriteData(fileName, data);
                currentPart++;
            }
        }
    }
}