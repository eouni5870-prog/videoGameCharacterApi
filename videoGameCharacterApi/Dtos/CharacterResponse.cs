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
        public int Strength { get; set; }
        public int Speed { get; set; }
        public int Magic { get; set; }
        public int Defense { get; set; }
        public DateTime CreatedAt { get; set; }
        public int GameId { get; set; }
        public string GameName { get; set; } = string.Empty;

        /// <summary>Average of the ratings (1-5), null when nobody rated the character yet.</summary>
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public int FavoriteCount { get; set; }
    }
}
