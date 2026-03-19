namespace RecipesAPI.DTOs
{
    public class OpinionDetailsDto
    {
        public int Id { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public AuthorDto Author { get; set; }
        public int RecipeId { get; set; }
        public string? RecipeName { get; set; }
    }
}
