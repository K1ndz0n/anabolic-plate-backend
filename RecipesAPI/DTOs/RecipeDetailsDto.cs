namespace RecipesAPI.DTOs
{
    public class RecipeDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Steps { get; set; }
        public float Rating { get; set; }
        public int OpinionAmount { get; set; }
        public AuthorDto Author { get; set; }
        public NutritionDetailsDto? Nutrition { get; set; }
        public List<IngredientDto> Ingredients { get; set; }
        public List<PhotoDto> PhotoUrls { get; set; }
    }
}
