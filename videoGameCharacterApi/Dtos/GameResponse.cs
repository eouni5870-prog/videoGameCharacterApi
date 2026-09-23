namespace videoGameCharacterApi.Dtos;

public class GameResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Genre { get; set; } = string.Empty;
    public int ReleaseYear { get; set; }
    public int CharacterCount { get; set; }
}
