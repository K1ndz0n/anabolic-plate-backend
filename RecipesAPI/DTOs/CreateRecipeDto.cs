using RecipesAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.DTOs
{
    public class CreateRecipeDto
    {
        [Length(1, 100, ErrorMessage = "Length must be between 1 and 100")]
        public string Name { get; set; }

        [MaxLength(255, ErrorMessage = "Length must be shorter than 255")]
        public string? Description { get; set; }

        [MaxLength(3000, ErrorMessage = "Length must be shorter than 3000")]
        public string? Steps { get; set; }
        public List<IngredientDto> Ingredients { get; set; }
    }
}
