using System.ComponentModel.DataAnnotations;

namespace videoGameCharacterApi.Dtos
{
    // The id comes from the route (PUT /api/VideoGameCharacters/{id}), so it is not part of the body.
    public class UpdateCharacterRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string Role { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Url, MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Range(1, 100)]
        public int Level { get; set; } = 1;

        [Range(1, int.MaxValue, ErrorMessage = "GameId is required.")]
        public int GameId { get; set; }
    }
}
