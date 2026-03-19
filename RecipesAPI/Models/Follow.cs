namespace RecipesAPI.Models
{
    public class Follow
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public AppUser User { get; set; }

        public string FollowedUserId { get; set; }
        public AppUser FollowedUser { get; set; }
    }
}
