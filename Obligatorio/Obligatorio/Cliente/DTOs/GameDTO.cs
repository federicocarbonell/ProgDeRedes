using System;
using System.Collections.Generic;
using System.Text;

namespace Client.DTOs
{
    public class GameDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Genre { get; set; }

        public string Description { get; set; }

        public string CoverPath { get; set; }

    }
}
