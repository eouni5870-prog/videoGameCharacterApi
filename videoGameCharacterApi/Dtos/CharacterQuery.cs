using System.ComponentModel.DataAnnotations;

namespace videoGameCharacterApi.Dtos;

/// <summary>Query string for GET /api/VideoGameCharacters?game=zelda&amp;role=hero&amp;page=1&amp;pageSize=10</summary>
public class CharacterQuery
{
    /// <summary>Part of the game name (case-insensitive).</summary>
    public string? Game { get; set; }

    public int? GameId { get; set; }

    /// <summary>Exact role, e.g. Hero or Villain (case-insensitive).</summary>
    public string? Role { get; set; }

    /// <summary>Part of the character name (case-insensitive).</summary>
    public string? Search { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
