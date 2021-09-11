using StateServices.DomainEntities;
using System;
using System.Collections.Generic;

namespace StateServices
{
    public class ServerState
    {

        private static readonly object Locker = new Object();

        //aca no se si no hacer lockers auxiliares para cada lista y en los gets lockear tambien, pregunta para Luis.
        private ServerState Instance { get; set; }

        public List<User> Users { get; set; }

        public List<Review> Reviews { get; set; }

        public List<Game> Games { get; set; }

        private ServerState() { }

        public ServerState GetInstance()
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
