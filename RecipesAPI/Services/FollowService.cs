using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.Models;

namespace RecipesAPI.Services
{
    public interface IFollowService
    {
        Task<Follow> CreateFollow(AppUser user, AppUser userToFollow);
        Task DeleteFollow(Follow follow, AppUser userToUnfollow);
        Task UpdateFollowCount(AppUser user);
        Task<bool> CheckForDuplicate(string userId, string userToFollowId);
    }
    public class FollowService : IFollowService
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;

        public FollowService(RecipesDbContext context, UserManager<AppUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }

        public async Task<Follow> CreateFollow(AppUser user, AppUser userToFollow)
        {
            Follow follow = new Follow
            {
                UserId = user.Id,
                FollowedUserId = userToFollow.Id
            };

            await context.Follows.AddAsync(follow);
            await context.SaveChangesAsync();
            await UpdateFollowCount(userToFollow);

            return follow;
        }

        public async Task DeleteFollow(Follow follow, AppUser userToUnfollow)
        {
            context.Follows.Remove(follow);
            await context.SaveChangesAsync();
            await UpdateFollowCount(userToUnfollow);
        }

        public async Task UpdateFollowCount(AppUser user)
        {
            user.FollowAmount = await context.Follows
                .CountAsync(f => f.FollowedUserId == user.Id);

            await userManager.UpdateAsync(user);
        }

        public async Task<bool> CheckForDuplicate(string userId, string userToFollowId)
        {
            var follow = await context.Follows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.FollowedUserId == userToFollowId);

            return follow != null;
        }
    }
}
