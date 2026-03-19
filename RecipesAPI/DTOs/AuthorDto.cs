namespace RecipesAPI.DTOs
{
    public class AuthorDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string? ProfileUrl { get; set; }
        public int FollowAmount { get; set; }
    }
}
