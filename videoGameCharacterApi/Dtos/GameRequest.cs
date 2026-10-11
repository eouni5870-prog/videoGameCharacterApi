using System.ComponentModel.DataAnnotations;

namespace videoGameCharacterApi.Dtos;

public class GameRequest
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Genre { get; set; } = string.Empty;

    [Range(1950, 2100)]
    public int ReleaseYear { get; set; }

    [Range(0, 1000)]
    public decimal Price { get; set; }

    [Url, MaxLength(500)]
    public string? CoverUrl { get; set; }
}
