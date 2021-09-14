﻿using StateServices.DomainEntities;
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

        //aca no se si no hacer lockers auxiliares para cada lista y en los gets lockear tambien, pregunta para Luis.
        private static ServerState Instance { get; set; }

        public List<User> Users 
        { 
            get
            {
                lock(UsersLocker)
                {
                    if (Users == null) return new List<User>();
                    return Users;
                }//no se si para lectura es necesario
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
                lock (ReviewsLocker)
                {
                    if (Reviews == null) return new List<Review>();
                    return Reviews;
                }//no se si para lectura es necesario
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
                lock (GamesLocker)
                {
                    if (Games == null) return new List<Game>();
                    return Games;
                }//no se si para lectura es necesario
            }
            set
            {
                lock (GamesLocker)
                {
                    Games = value;
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
                    return new ServerState();
                }
            }
            return Instance;
        }

    }
}
