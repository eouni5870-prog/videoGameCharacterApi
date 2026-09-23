namespace videoGameCharacterApi.Dtos
{
    public class CharacterResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Level { get; set; }
        public DateTime CreatedAt { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;
    }
}
