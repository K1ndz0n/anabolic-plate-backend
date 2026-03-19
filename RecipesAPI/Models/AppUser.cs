using Microsoft.AspNetCore.Identity;

namespace RecipesAPI.Models
{
    public class AppUser : IdentityUser
    {
        public int FollowAmount { get; set; }
        public List<Recipe> Recipes { get; set; } = new();
        public List<Opinion> Opinions { get; set; } = new();
        public List<Follow> Follows { get; set; } = new();
    }
}
