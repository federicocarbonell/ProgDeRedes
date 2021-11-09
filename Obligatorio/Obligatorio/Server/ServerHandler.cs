using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common;
using DTOs;
using Grpc.Net.Client;
using ProtocolLibrary;
using StateServices;

namespace Server
{
    public class ServerHandler
    {
        private TcpClient tcpClient;
        private readonly TcpListener tcpListener;
        private GrpcChannel channel;
        private Game.GameClient client;

        public ServerHandler(TcpClient tcpClient, TcpListener tcpListener)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            this.tcpClient = tcpClient;
            this.tcpListener = tcpListener;
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            client = new Game.GameClient(channel);
        }

        public async Task<string> AddGameAsync(GameDTO game)
        {
            var reply = await client.AddGameAsync(new GameMessage { Id = 0, Name = game.Name, CoverPath = game.Name + ".png", Genre = game.Genre, Description = game.Description });
            return reply.Message;
        }

        public async Task<string> GetGamesAsync()
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();
            var gamesList = await client.GetAllGamesAsync(request).ResponseAsync;
            return ConvertToString(gamesList);
        }

        private string ConvertToString(GamesList list)
        {
            string aux = "";

            foreach (GameMessage game in list.Games)
            {
                aux += $"Id: {game.Id} Nombre: {game.Name} \n";
            }

            return aux;
        }

        public async Task<bool> DoLoginAsync(byte[] bufferData, AuthenticationService authService)
        {
            int usernameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string username = convertToString(bufferData, usernameLength, beforeLength);

            beforeLength += usernameLength + 4;
            int passwordLength = obtainLength(bufferData, beforeLength);
            string pass = convertToString(bufferData, passwordLength, beforeLength);

            return authService.Login(username, pass);
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

        public async Task AddCoverGameAsync(byte[] bufferData, TcpClient tcpClient)
        {
            int fileNameLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            string fileName = convertToString(bufferData, fileNameLength, beforeLength);

            beforeLength += fileNameLength + 4;
            int fileSizeLength = obtainLength(bufferData, beforeLength);
            long fileSize = convertToLong(bufferData, fileSizeLength, beforeLength);
            await ReceiveFileAsync(fileName, tcpClient);
        }

        public int RecieveBuyerInfo(byte[] bufferData)
        {
            int idLength = obtainLength(bufferData, 0);
            int beforeLength = 0;
            int gameId = convertToInt(bufferData, idLength, beforeLength);

            return gameId;
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

        public async Task SendFileAsync(string path, TcpClient tcpClient)
        {
            var fileCommunication = new FileCommunicationHandler(tcpClient);
            await fileCommunication.SendFileAsync(path);
        }

        private async Task ReceiveFileAsync(string fileName, TcpClient tcpClient)
        {
            var fileCommunication = new FileCommunicationHandler(tcpClient);
            await fileCommunication.ReceiveFileAsync(fileName);
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
