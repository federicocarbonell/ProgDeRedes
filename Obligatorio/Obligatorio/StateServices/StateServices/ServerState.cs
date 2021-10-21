using StateServices.DomainEntities;
using System;
using System.Collections.Generic;

namespace StateServices
{
    public class ServerState
    {

        private static readonly object Locker = new Object();
        private static readonly object UsersLocker = new Object();
        private static readonly object ReviewsLocker = new Object();
        private static readonly object GamesLocker = new Object();

        // TODO lockear los gets tambien.
        private static ServerState Instance { get; set; }
        private List<Game> _Games { get; set; }

        public List<User> Users 
        { 
            get
            {
                if (Users == null) return new List<User>();
                return Users;
                
            }
            set
            {
                lock (UsersLocker)
                {
                    Users = value;
                }
            }
        }

        public List<Review> Reviews 
        {
            get
            {
                if (Reviews == null) return new List<Review>();
                return Reviews;
            }
            set
            {
                lock (ReviewsLocker)
                {
                    Reviews = value;
                }
            }
        }

        public List<Game> Games 
        {
            get
            {
                if (_Games == null) return new List<Game>();
                return _Games;
            }
            set
            {
                lock (GamesLocker)
                {
                    _Games = value;
                }
            }
        }

        private ServerState() { }

        public static ServerState GetInstance()
        {
            if (Instance == null)
            {
                lock (Locker)
                {
                    Instance = new ServerState();
                }
            }
            return Instance;
        }

    }
}
