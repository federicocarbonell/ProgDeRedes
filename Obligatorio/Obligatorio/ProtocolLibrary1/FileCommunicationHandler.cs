using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using ProtocolLibrary;

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

        public async Task SendFileAsync(string path)
        {
            var fileInfo = new FileInfo(path);
            string fileName = fileInfo.Name;
            byte[] fileNameData = Encoding.UTF8.GetBytes(fileName);
            int fileNameLength = fileNameData.Length;
            byte[] fileNameLengthData = BitConverter.GetBytes(fileNameLength);
            // 1.- Envío el largo del nombre del archivo
            await _networkStreamHandler.SendDataAsync(fileNameLengthData);
            // 2.- Envío el nombre del archivo
            await _networkStreamHandler.SendDataAsync(fileNameData);

            long fileSize = fileInfo.Length;
            byte[] fileSizeDataLength = BitConverter.GetBytes(fileSize);
            // 3.- Envío el largo del archivo
            await _networkStreamHandler.SendDataAsync(fileSizeDataLength);
            // 4.- Envío los datos del archivo
            await SendFileAsync(fileSize, path);
        }

        public async Task ReceiveFileAsync(string fileName)
        {
            //1.- Recibir el largo del nombre del archivo
            byte[] fileNameLengthData = await _networkStreamHandler.ReadDataAsync(ProtocolSpecification.FileNameSize);
            int fileNameLength = BitConverter.ToInt32(fileNameLengthData);
            // 2.- Recibir el nombre del archivo original, se va a descartar igual para ponerle el nombre del juego.
            byte[] fileNameData = await _networkStreamHandler.ReadDataAsync(fileNameLength);
            string fileName1 = Encoding.UTF8.GetString(fileNameData);

            // 3.- Recibo el largo del archivo
            byte[] fileSizeDataLength = await _networkStreamHandler.ReadDataAsync(ProtocolSpecification.FileSize);
            long fileSize = BitConverter.ToInt64(fileSizeDataLength);

            if (fileName == "") fileName = fileName1;
            await ReceiveFileAsync(fileSize, fileName);
        }

        private async Task SendFileAsync(long fileSize, string path)
        {
            long fileParts = ProtocolSpecification.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;

            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _fileStreamHandler.ReadDataAsync(path, ProtocolSpecification.MaxPacketSize, offset);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await _fileStreamHandler.ReadDataAsync(path, lastPartSize, offset);
                    offset += lastPartSize;
                }

                await _networkStreamHandler.SendDataAsync(data);
                currentPart++;
            }
        }

        private async Task ReceiveFileAsync(long fileSize, string fileName)
        {
            long fileParts = ProtocolSpecification.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await _networkStreamHandler.ReadDataAsync(ProtocolSpecification.MaxPacketSize);
                    offset += ProtocolSpecification.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await _networkStreamHandler.ReadDataAsync(lastPartSize);
                    offset += lastPartSize;
                }
                await _fileStreamHandler.WriteDataAsync(fileName, data);
                currentPart++;
            }
        }
    }
}