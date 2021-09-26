using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Client.DTOs;
using ProtocolLibrary;

namespace Server
{
    public class ServerHandler
    {
        private readonly Socket socket;
        private readonly SocketStreamHandler socketStreamHandler;

        public ServerHandler(Socket socket)
        {
            this.socket = socket;
            socketStreamHandler = new SocketStreamHandler(this.socket);
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

            //beforeLength += descriptionLength + 4;
            //int coverLength = obtainLength(bufferData, beforeLength);
            //string coverPath = convertToString(bufferData, coverLength, beforeLength);

            return new GameDTO { Name = name , Genre = genre, Description = description};
        }

        public void AddCoverGame(Socket clientSocket, byte[] bufferData)
        {
            int fileNameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string fileName = convertToString(bufferData, fileNameLength, beforeLength);

            beforeLength += fileNameLength + 4;
            int fileSizeLength = obtainLength(bufferData, beforeLength);
            long fileSize = convertToLong(bufferData, fileSizeLength, beforeLength);
            ReceiveFile(clientSocket, fileSize, fileName);
            //game.CoverPath = fileName;
            //return game;
        }


        public GameDTO ReceiveGameForModifying (byte[] bufferData)
        {

            int idLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int id = convertToInt(bufferData, idLength, beforeLength);

            beforeLength = idLength + 4;
            int nameLength = obtainLength(bufferData, beforeLength);
            string name = convertToString(bufferData, nameLength, beforeLength);

            beforeLength = nameLength + 4;
            int genreLength = obtainLength(bufferData, beforeLength);
            string genre = convertToString(bufferData, genreLength, beforeLength);

            beforeLength += genreLength + 4;
            int descriptionLength = obtainLength(bufferData, beforeLength);
            string description = convertToString(bufferData, descriptionLength, beforeLength);

            beforeLength += descriptionLength + 4;
            int coverLength = obtainLength(bufferData, beforeLength);
            string coverPath = convertToString(bufferData, coverLength, beforeLength);

            return new GameDTO { Id = id, Name = name, Genre = genre, Description = description, CoverPath = coverPath };
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

        private void ReceiveFile(Socket clientSocket, long fileSize, string fileName)
        {
            long fileParts = FileTransferProtocol.CalculateParts(fileSize);
            long offset = 0;
            long currentPart = 1;
            while (fileSize > offset)
            {
                byte[] data;
                if (currentPart != fileParts)
                {
                    data = socketStreamHandler.ReceiveData(clientSocket, FileTransferProtocol.MaxPacketSize);
                    offset += FileTransferProtocol.MaxPacketSize;
                }
                else
                {
                    int lastPartSize = (int)(fileSize - offset);
                    data = socketStreamHandler.ReceiveData(clientSocket, lastPartSize);
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
