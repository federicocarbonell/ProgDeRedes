using System;
namespace Client.Interfaces
{
    public interface IClientHandler
    {

        public void AddGame(string title, string genre, string trailer, string cover);
        public void DeleteGame(int id);
        public void ModifyGame(int id, string title, string genre, string trailer, string cover);
        public void QualifyGame(int id, int rating, string review);
        public void ViewGameDetail(int id);
        public void ViewGames();
    }
}
