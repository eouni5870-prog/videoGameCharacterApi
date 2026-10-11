using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using videoGameCharacterApi.Data;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Models;

namespace videoGameCharacterApi.Services;

public class ReviewService(AppDbContext context, ILogger<ReviewService> logger) : IReviewService
{
    private static readonly Expression<Func<Review, ReviewResponse>> ToResponse = r => new ReviewResponse
    {
        Id = r.Id,
        Username = r.User.Username,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };

    public async Task<List<ReviewResponse>> GetForCharacterAsync(int characterId)
        => await context.Reviews
            .AsNoTracking()
            .Where(r => r.CharacterId == characterId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .Select(ToResponse)
            .ToListAsync();

    public async Task<ReviewResponse?> SaveAsync(int userId, int characterId, ReviewRequest request)
    {
        if (!await context.Characters.AnyAsync(c => c.Id == characterId))
            return null;

        var comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim();

        var review = await context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.CharacterId == characterId);
        if (review is null)
        {
            review = new Review { UserId = userId, CharacterId = characterId, Rating = request.Rating, Comment = comment };
            context.Reviews.Add(review);
        }
        else
        {
            review.Rating = request.Rating;
            review.Comment = comment;
            review.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
        logger.LogInformation("User {UserId} rated character {CharacterId}: {Rating}/5", userId, characterId, request.Rating);

        return await context.Reviews.AsNoTracking().Where(r => r.Id == review.Id).Select(ToResponse).FirstAsync();
    }

    public async Task<bool> DeleteAsync(int userId, int characterId)
    {
        var review = await context.Reviews.FirstOrDefaultAsync(r => r.UserId == userId && r.CharacterId == characterId);
        if (review is null)
            return false;

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();
        return true;
    }
}
