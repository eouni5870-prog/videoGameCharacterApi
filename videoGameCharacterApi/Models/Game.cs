namespace videoGameCharacterApi.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }

    // Shop
    public decimal Price { get; set; }
    public string? CoverUrl { get; set; }

    public List<Character> Characters { get; set; } = [];
}
