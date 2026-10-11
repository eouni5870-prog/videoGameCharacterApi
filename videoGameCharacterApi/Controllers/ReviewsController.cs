using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using videoGameCharacterApi.Dtos;
using videoGameCharacterApi.Extensions;
using videoGameCharacterApi.Services;

namespace videoGameCharacterApi.Controllers;

/// <summary>Ratings and comments of a character: /api/VideoGameCharacters/{characterId}/reviews</summary>
[Route("api/VideoGameCharacters/{characterId:int}/reviews")]
[ApiController]
[Authorize]
public class ReviewsController(IReviewService service) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<ReviewResponse>>> GetReviews(int characterId)
        => Ok(await service.GetForCharacterAsync(characterId));

    // PUT = create or replace my review of this character
    [HttpPut]
    public async Task<ActionResult<ReviewResponse>> SaveMyReview(int characterId, ReviewRequest request)
    {
        if (User.GetUserId() is not int userId) return Unauthorized();

        var review = await service.SaveAsync(userId, characterId, request);
        return review is null
            ? Problem(detail: $"Character with id {characterId} was not found.", statusCode: StatusCodes.Status404NotFound)
            : Ok(review);
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteMyReview(int characterId)
    {
        if (User.GetUserId() is not int userId) return Unauthorized();

        return await service.DeleteAsync(userId, characterId)
            ? NoContent()
            : Problem(detail: "You have not reviewed this character.", statusCode: StatusCodes.Status404NotFound);
    }
}
