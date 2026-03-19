using System.Security.Cryptography.Pkcs;

namespace RecipesAPI.DTOs
{
    public class RecipeThumbnailDto
    {
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public float? Rating { get; set; }
        public int OpinionAmount { get; set; }
        public AuthorDto Author { get; set; }
        public string? ThumbnailPhotoUrl { get; set; }
    }
}
