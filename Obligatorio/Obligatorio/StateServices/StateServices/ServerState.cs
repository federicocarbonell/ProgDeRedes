using StateServices.DomainEntities;
using System;
using System.Collections.Generic;

namespace StateServices
{
    public class ServerState
    {

        private static readonly object Locker = new Object();
        private static readonly object UsersWriteLocker = new Object();
        private static readonly object ReviewsWriterLocker = new Object();
        private static readonly object GamesWriterLocker = new Object();
        private static readonly object UsersReadLocker = new Object();
        private static readonly object ReviewsReadLocker = new Object();
        private static readonly object GamesReadLocker = new Object();

        private static ServerState Instance { get; set; }
        private List<Game> _Games { get; set; }

        public List<User> Users 
        { 
            get
            {
                lock (UsersWriteLocker)
                {
                    if (Users == null) return new List<User>();
                    return Users;
                }
                
            }
            set
            {
                lock (UsersReadLocker)
                {
                    Users = value;
                }
            }
        }

        public List<Review> Reviews 
        {
            get
            {
                lock (ReviewsWriterLocker)
                {
                    if (Reviews == null) return new List<Review>();
                    return Reviews;
                }
            }
            set
            {
                lock (ReviewsReadLocker)
                {
                    Reviews = value;
                }
            }
        }

        public List<Game> Games 
        {
            get
            {
                lock (GamesWriterLocker)
                {
                    if (_Games == null) return new List<Game>();
                    return _Games;
                }
            }
            set
            {
                lock (GamesReadLocker)
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
