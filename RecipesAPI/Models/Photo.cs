namespace RecipesAPI.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Path { get; set; }

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
    }
}
