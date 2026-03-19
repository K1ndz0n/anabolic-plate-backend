namespace RecipesAPI.DTOs
{
    public class FollowDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FollowedUserId { get; set; }
    }
}
