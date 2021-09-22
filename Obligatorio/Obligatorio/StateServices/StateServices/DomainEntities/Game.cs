using System;
using System.Collections.Generic;
using System.Text;

namespace StateServices.DomainEntities
{
    public class Game
    {
        //usamos esto de Id?
        public int Id { get; set; }
        public string Name { get; set; }

        public string Genre { get; set; }

        public double Rating { get; set; }

        public string Description { get; set; }
        //aca no se si guardamos la referencia a la ruta en disco o que 
        public string CoverPath { get; set; }

        public bool isDeleted { get; set; }

        public List<Review> Reviews { get; set; }

    }
}
