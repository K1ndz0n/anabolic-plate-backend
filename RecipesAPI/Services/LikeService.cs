using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.Models;

namespace RecipesAPI.Services
{
    public interface ILikeService
    {
        Task<Like> CreateLike(AppUser user, int recipeId);
        Task DeleteLike(Like like);
        Task<bool> CheckForDuplicate(string userId, int recipeId);
    }

    public class LikeService : ILikeService
    {
        private readonly RecipesDbContext context;

        public LikeService(RecipesDbContext context)
        {
            this.context = context;
        }

        public async Task<Like> CreateLike(AppUser user, int recipeId)
        {
            Like like = new Like
            {
                UserId = user.Id,
                RecipeId = recipeId
            };

            await context.Likes.AddAsync(like);
            await context.SaveChangesAsync();

            return like;
        }

        public async Task DeleteLike(Like like)
        {
            context.Likes.Remove(like);
            await context.SaveChangesAsync();
        }

        public async Task<bool> CheckForDuplicate(string userId, int recipeId)
        {
            var like = await context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.RecipeId == recipeId);

            return like != null;
        }
    }
}
