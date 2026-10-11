using System.Security.Claims;

namespace videoGameCharacterApi.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>Id of the logged-in user, read from the JWT ("sub" claim). Null when not logged in.</summary>
    public static int? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return int.TryParse(value, out var id) ? id : null;
    }
}
