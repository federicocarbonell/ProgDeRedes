﻿using StateServices.DomainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StateServices
{
    public class GameRepository : IRepository<Game>
    {

        public GameRepository() { }

        public void Add(Game entity)
        {
            var auxList = ServerState.GetInstance().Games;
            entity.isDeleted = false;
            entity.Id = obtainId();
            auxList.Add(entity);
            ServerState.GetInstance().Games = auxList;
        }//hay que hacer esta magia con las listas en todos creo

        public int obtainId()
        {
            var auxList = ServerState.GetInstance().Games;
            var id = auxList.Count() + 1;
            return id;
        }

        public void Delete(int id)
        {
            int arrPos = ServerState.GetInstance().Games.FindIndex(x => x.Id == id);
            var auxList = ServerState.GetInstance().Games;
            auxList.Find(x => x.Id == id).isDeleted = true;
            ServerState.GetInstance().Games = auxList;
        }

        public void QualifyGame(int id, int rating, string review)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            Game game = Get(id);
            Review newReview = CreateReview(id, rating, review);
            var auxList = ServerState.GetInstance().Games;
            auxList.Find(x => x.Id == id).Reviews = new List<Review> { newReview};
        }

        public Review CreateReview(int gameId, int rating, string review)
        {
            if (!ValidId(gameId))
                throw new Exception("Given id has no corresponding game");
            Game game = Get(gameId);
            int reviewId = game.Reviews != null ? game.Reviews.Count() + 1 : 1;
            Review newReview = new Review{ Id = reviewId, Rating = rating, Content =review};
            return newReview;
        }

        public Game Get(int id)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            return ServerState.GetInstance().Games.Find(x => x.Id == id);
        }

        private bool ValidId(int id)
        {
            return id <= ServerState.GetInstance().Games.Count();
        }

        public IQueryable<Game> GetAll()
        {
            return ServerState.GetInstance().Games.ToList().AsQueryable();
        }

        public void Update(int id, Game newEntity)
        {
            if (!ValidId(id))
                throw new Exception("Given id has no corresponding game");
            Game old = Get(id);
            old.Name = newEntity.Name;
            old.Genre = newEntity.Genre;
            old.CoverPath = newEntity.CoverPath;
            old.Description = newEntity.Description;
        }

    }

}
