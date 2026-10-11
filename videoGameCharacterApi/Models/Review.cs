namespace videoGameCharacterApi.Models;

/// <summary>A rating (1 to 5 stars) and an optional comment. One review per user and per character.</summary>
public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int CharacterId { get; set; }
    public Character Character { get; set; } = null!;

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
