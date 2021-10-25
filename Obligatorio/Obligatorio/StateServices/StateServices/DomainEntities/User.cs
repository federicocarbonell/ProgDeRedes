using System;
using System.Collections.Generic;
using System.Text;

namespace StateServices.DomainEntities
{
    public class User
    {

        public int Id { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsDeleted { get; set; }

        public List<Game> OwnedGames { get; set; } 

    }
}
