using RecipesAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.DTOs
{
    public class IngredientDto
    {
        [Length(1, 50, ErrorMessage = "Length must be between 1 and 50")]
        public string Name { get; set; }

        [Range(0.0001, float.MaxValue)]
        public float Amount { get; set; }

        [EnumDataType(typeof(Unit))]
        public Unit Unit { get; set; }
    }
}
