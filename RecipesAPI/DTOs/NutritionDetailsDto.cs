using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.DTOs
{
    public class NutritionDetailsDto
    {
        public float Protein { get; set; }
        public float Carbs { get; set; }
        public float Fat { get; set; }
        public float Kcal { get; set; }
    }
}
