using DTOs;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StateServer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StateServer.Services
{
    public class ReviewService : Reviews.ReviewsBase
    {
        private readonly ILogger<GameService> _logger;
        private readonly IRepository<ReviewDTO> ReviewRepository;

        public ReviewService(ILogger<GameService> logger, IRepository<ReviewDTO> reviewRepo)
        {
            _logger = logger;
            ReviewRepository = reviewRepo;
        }

        public override Task<AddReviewResponse> AddReview(ReviewMessage message, ServerCallContext context)
        {
            try
            {
                ReviewRepository.GetInstance().Add(FromMessage(message));
                return Task.FromResult(new AddReviewResponse
                {
                    Message = $"Reviewed game with id {message.GameId} successfully"
                });
            }
            catch (Exception e)
            {
                return Task.FromResult(new AddReviewResponse
                {
                    Message = e.Message
                });
            }
        }

        public override Task<ReviewMessageList> GetReviewsByGameId(GameIdMessage request, ServerCallContext context)
        {
            try
            {
                var reviews = ReviewRepository.GetInstance().GetAll().Where(x => x.GameId == request.Id);
                var list = new ReviewMessageList();

                foreach (var review in reviews)
                {
                    list.Reviews.Add(new ReviewMessage { Id = review.Id, GameId = review.GameId, Rating = review.Rating, Content = review.Content });
                }

                return Task.FromResult(list);
            }
            catch (Exception e)
            {
                return Task.FromResult(new ReviewMessageList
                {
                });
            }
        }

        private ReviewDTO FromMessage(ReviewMessage message)
        {
            return new ReviewDTO
            {
                Id = message.Id,
                GameId = message.GameId,
                Rating = message.Rating,
                Content = message.Content
            };
        }

    }
}
