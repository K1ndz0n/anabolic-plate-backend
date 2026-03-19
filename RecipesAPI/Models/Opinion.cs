using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.Models
{
    public class Opinion
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
