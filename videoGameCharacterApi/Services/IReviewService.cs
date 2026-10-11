using videoGameCharacterApi.Dtos;

namespace videoGameCharacterApi.Services;

public interface IReviewService
{
    Task<List<ReviewResponse>> GetForCharacterAsync(int characterId);

    /// <summary>Creates or updates the user's review. Returns null when the character does not exist.</summary>
    Task<ReviewResponse?> SaveAsync(int userId, int characterId, ReviewRequest request);

    /// <summary>Returns false when the user had no review for this character.</summary>
    Task<bool> DeleteAsync(int userId, int characterId);
}
