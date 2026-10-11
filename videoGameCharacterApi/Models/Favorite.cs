namespace videoGameCharacterApi.Models;

/// <summary>A character marked as favorite by a user (many-to-many between User and Character).</summary>
public class Favorite
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
