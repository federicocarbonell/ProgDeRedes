using System;
using Client.DTOs;

namespace Server
{
    public class ServerHandler
    {
        public ServerHandler()
        {
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

            beforeLength += descriptionLength + 4;
            int coverLength = obtainLength(bufferData, beforeLength);
            string coverPath = convertToString(bufferData, coverLength, beforeLength);

            return new GameDTO { Name = name , Genre = genre, Description = description, CoverPath = coverPath};
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

    }
}
