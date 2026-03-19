using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.DTOs
{
    public class CreateNutritionDto
    {
        [Range(0, float.MaxValue)]
        public float Protein { get; set; }
        [Range(0, float.MaxValue)]
        public float Carbs { get; set; }
        [Range(0, float.MaxValue)]
        public float Fat { get; set; }
    }
}
