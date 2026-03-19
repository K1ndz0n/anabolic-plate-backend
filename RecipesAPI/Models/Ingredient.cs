using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.Models
{
    public enum Unit
    {
        Gram,
        Ml,
        Piece,
        Tsp,
        Tbsp,
        Cup,
        Pinch
    }

    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [Range(0.0001, float.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public float Amount { get; set; }

        [EnumDataType(typeof(Unit))]
        public Unit Unit { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
