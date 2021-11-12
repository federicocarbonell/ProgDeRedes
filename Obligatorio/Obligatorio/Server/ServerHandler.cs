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
        private Game.GameClient gameClient;
        private Reviews.ReviewsClient reviewClient;

        public ServerHandler(TcpClient tcpClient, TcpListener tcpListener)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            this.tcpClient = tcpClient;
            this.tcpListener = tcpListener;
            channel = GrpcChannel.ForAddress("http://localhost:5000");
            gameClient = new Game.GameClient(channel);
            reviewClient = new Reviews.ReviewsClient(channel);
        }

        public async Task<string> AddGameAsync(GameDTO game)
        {
            var reply = await gameClient.AddGameAsync(new GameMessage { Id = 0, Name = game.Name, CoverPath = game.Name + ".png", Genre = game.Genre, Description = game.Description, IsDeleted = false });
            return reply.Message;
        }

        public async Task<string> GetGamesAsync()
        {
            var request = new Google.Protobuf.WellKnownTypes.Empty();
            var gamesList = await gameClient.GetAllGamesAsync(request).ResponseAsync;
            return ConvertToString(gamesList);
        }

        public async Task<bool> DeleteGameAsync (int gameId)
        {
            var response = await gameClient.DeleteGameAsync(new GameId { Id = gameId });
            return response.Response;
        }

        public async Task<string> GetGameDetailAsync(int gameId)
        {
            var gameInfo = await gameClient.GetGameDetailAsync(new GameId { Id = gameId });
            var reviews = await reviewClient.GetReviewsByGameIdAsync(new GameIdMessage { Id = gameId });

            string details = "";
            details += $"Id: {gameInfo.Id}, Nombre: {gameInfo.Name} \n";
            details += $"Categoria: {gameInfo.Genre} , Descripcion: {gameInfo.Description} \n";

            double rating = 0;
            int counter = 0;

            details += "Reviews: \n";
            foreach (var review in reviews.Reviews)
            {
                details += $"Id: {review.Id}, Rating: {review.Rating}" +
                    $", Reseña: {review.Content}, \n";
                rating += review.Rating;
                counter++;
            }
            details += $"Rating promedio: {rating / counter} \n";

            return details;
        }

        public async Task<string> QualifyGameAsync(ReviewDTO review)
        {
            var response = await reviewClient.AddReviewAsync(new ReviewMessage
            {
                Content = review.Content,
                GameId = review.GameId,
                Rating = review.Rating,
                Id = review.Id
            });
            return response.Message;
        }

        public async Task<bool> ModifyGameAsync (int gameId, GameDTO game)
        {
            var response = await gameClient.ModifyGameAsync(new ModifyGameRequest
            {
                GameId = gameId,
                Game = new GameMessage
                {
                    Id = game.Id,
                    Name = game.Name,
                    Description = game.Description,
                    Genre = game.Genre,
                }
            });
            return response.Response;
        }

        public async Task<string> SearchForGameAsync(Tuple<int, string, int> data)
        {
            if (data.Item1 == 1 || data.Item1 == 2)
            {
                var response = await gameClient.GetAllByQueryAsync(new GetAllByQueryRequest
                {
                    QueryType = data.Item1,
                    TextQueryData = data.Item2,
                    RatingQueryData = data.Item3
                });
                return response.Message;
            }
            else
            {
                return "";
            }

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
