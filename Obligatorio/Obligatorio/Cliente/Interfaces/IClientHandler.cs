using System;
using System.Threading.Tasks;

namespace Client.Interfaces
{
    public interface IClientHandler
    {

        public Task AddGame(string title, string genre, string trailer, string cover);
        public Task DeleteGame(int id);
        public Task ModifyGame(int id, string title, string genre, string trailer, string cover);
        public Task QualifyGame(int id, int rating, string review);
        public Task ViewGameDetail(int id);
        public Task ViewGames();
    }
}
