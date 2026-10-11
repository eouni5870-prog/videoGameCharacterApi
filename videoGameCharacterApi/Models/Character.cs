namespace videoGameCharacterApi.Models;

public class Character
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int Level { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Stats from 0 to 100 (shown as a radar chart and used to compare characters)
    public int Strength { get; set; }
    public int Speed { get; set; }
    public int Magic { get; set; }
    public int Defense { get; set; }

    // Relation: each character belongs to one game
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    public List<Review> Reviews { get; set; } = [];
    public List<Favorite> Favorites { get; set; } = [];
}
