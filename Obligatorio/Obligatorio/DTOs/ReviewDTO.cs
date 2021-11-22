using System;
namespace DTOs
{
    public class ReviewDTO
    {
        public int Id { get; set; }
        public int GameId { get; set; }

        public int Rating { get; set; }

        public string Content { get; set; }
    }
}
