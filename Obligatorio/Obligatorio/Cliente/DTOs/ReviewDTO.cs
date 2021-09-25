using System;
namespace Client.DTOs
{
    public class ReviewDTO
    {
        public int GameId { get; set; }

        public int Rating { get; set; }

        public string Content { get; set; }
    }
}
