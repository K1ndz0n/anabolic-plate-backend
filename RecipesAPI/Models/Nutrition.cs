namespace RecipesAPI.Models
{
    public class Nutrition
    {
        public int Id { get; set; }
        public float Protein { get; set; }
        public float Carbs { get; set; }
        public float Fat { get; set; }
        public float Kcal { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
