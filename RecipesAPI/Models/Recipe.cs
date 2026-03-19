 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecipesAPI.Models
{
    public class Recipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Steps { get; set; }
        public float Rating { get; set; }
        public int OpinionAmount { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public Nutrition? Nutrition { get; set; }
        public List<Ingredient> Ingredients { get; set; } = new();
        public List<Photo> Photos { get; set; } = new();
        public List<Opinion> Opinions { get; set; } = new();
        public List<Like> Likes { get; set; } = new();
    }
}
