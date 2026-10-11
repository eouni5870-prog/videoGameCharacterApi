namespace videoGameCharacterApi.Models;

/// <summary>The two roles of the application.</summary>
public static class Roles
{
    /// <summary>Manages the catalog (characters, games) and the users.</summary>
    public const string Admin = "Admin";

    /// <summary>Normal player: favorites, reviews, purchases.</summary>
    public const string User = "User";

    public static bool IsValid(string role) => role is Admin or User;
}
