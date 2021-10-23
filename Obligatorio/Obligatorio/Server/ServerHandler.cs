using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using DTOs;
using ProtocolLibrary;

namespace Server
{
    public class ServerHandler
    {
        private readonly TcpClient tcpClient;
        private readonly NetworkStreamHandler networkStreamHandler;

        public ServerHandler(TcpClient tcpClient)
        {
            this.tcpClient = tcpClient;
            networkStreamHandler = new NetworkStreamHandler(this.tcpClient);
        }

        public GameDTO ReceiveGame(byte[] bufferData)
        {
            int nameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string name = convertToString(bufferData, nameLength, beforeLength);

            beforeLength = nameLength + 4;
            int genreLength = obtainLength(bufferData, beforeLength);
            string genre = convertToString(bufferData, genreLength, beforeLength);

            beforeLength += genreLength + 4;
            int descriptionLength = obtainLength(bufferData, beforeLength);
            string description = convertToString(bufferData, descriptionLength, beforeLength);

            return new GameDTO { Name = name , Genre = genre, Description = description};
        }

        public string ReceiveOwnerName(byte[] bufferData)
        {
            int nameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string name = convertToString(bufferData, nameLength, beforeLength);

            return name;
        }

        public async Task AddCoverGameAsync(byte[] bufferData)
        {
            int fileNameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string fileName = convertToString(bufferData, fileNameLength, beforeLength);

            beforeLength += fileNameLength + 4;
            int fileSizeLength = obtainLength(bufferData, beforeLength);
            long fileSize = convertToLong(bufferData, fileSizeLength, beforeLength);
            await ReceiveFileAsync(fileSize, fileName);
        }

        public Tuple<int, string> RecieveBuyerInfo(byte[] bufferData)
        {
            int nameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int gameId = convertToInt(bufferData, nameLength, beforeLength);

            beforeLength = nameLength + 4;
            int genreLength = obtainLength(bufferData, beforeLength);
            string username = convertToString(bufferData, genreLength, beforeLength);

            return new Tuple<int, string>(gameId, username);
        }


        public GameDTO ReceiveGameForModifying (byte[] bufferData)
        {

            int idLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int id = convertToInt(bufferData, idLength, beforeLength);

            beforeLength += idLength + 4;
            int nameLength = obtainLength(bufferData, beforeLength);
            string name = convertToString(bufferData, nameLength, beforeLength);

            beforeLength += nameLength + 4;
            int genreLength = obtainLength(bufferData, beforeLength);
            string genre = convertToString(bufferData, genreLength, beforeLength);

            beforeLength += genreLength + 4;
            int descriptionLength = obtainLength(bufferData, beforeLength);
            string description = convertToString(bufferData, descriptionLength, beforeLength);

            return new GameDTO { Id = id, Name = name, Genre = genre, Description = description};
        }

        public Tuple<int, string, int> ReceiveSearchTerms(byte[] bufferData)
        {

            int idLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int id = convertToInt(bufferData, idLength, beforeLength);

            beforeLength += idLength + 4;
            int nameLength = obtainLength(bufferData, beforeLength);
            string name = convertToString(bufferData, nameLength, beforeLength);

            int minRating = 0;
            if (id == 3)
            {
                beforeLength += nameLength + 4;
                int ratingLength = obtainLength(bufferData, beforeLength);
                minRating = convertToInt(bufferData, ratingLength, beforeLength);
            }

            return new Tuple<int, string, int>(id, name, minRating);
        }

        public ReviewDTO ReceiveQualification(byte[] bufferData)
        {
            int idLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int gameId = convertToInt(bufferData, idLength, beforeLength);

            beforeLength += idLength + 4;
            int ratingLength = obtainLength(bufferData, beforeLength);
            int rating = convertToInt(bufferData, ratingLength, beforeLength);

            beforeLength += ratingLength + 4;
            int reviewLength = obtainLength(bufferData, beforeLength);
            string review = convertToString(bufferData, reviewLength, beforeLength);

            return new ReviewDTO { GameId = gameId, Rating = rating, Content = review };
        }

        public int ReceiveId(byte[] bufferData)
        {
            int idLength = obtainLength(bufferData, 0);
            return convertToInt(bufferData, idLength, 0);
        }

        private async Task ReceiveFileAsync(long fileSize, string fileName)
        {
            long fileParts = FileTransferProtocol.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = await networkStreamHandler.ReceiveDataAsync(FileTransferProtocol.MaxPacketSize);
                    offset += FileTransferProtocol.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = await networkStreamHandler.ReceiveDataAsync(lastPartSize);
                    offset += lastPartSize;
                }
                FileStreamHandler.WriteData(fileName, data);
                currentPart++;
            }
        }

        public int obtainLength(byte[] bufferData, int start)
        {
            return Convert.ToInt32(bufferData[start]);
        }

        public String convertToString(byte[] bufferData,int stringLength, int start)
        {
            byte[] stringBytes = new byte[stringLength];
            for (int i = 0; i < stringLength; i++)
            {
                stringBytes[i] = bufferData[i + start + 4];
            }
            return System.Text.Encoding.UTF8.GetString(stringBytes);
        }

        public int convertToInt(byte[] bufferData, int intLength, int start)
        {
            byte[] intBytes = new byte[intLength];
            for (int i = 0; i < intLength; i++)
            {
                intBytes[i] = bufferData[i + start + 4];
            }
            return BitConverter.ToInt32(intBytes);
        }

        private long convertToLong(byte[] bufferData, int longLength, int start)
        {
            byte[] longBytes = new byte[longLength];
            for (int i = 0; i < longLength; i++)
            {
                longBytes[i] = bufferData[i + start + 4];
            }
            return BitConverter.ToInt64(longBytes);
        }

    }
}
