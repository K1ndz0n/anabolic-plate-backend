using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;
using RecipesAPI.Services.Utils;

namespace RecipesAPI.Services
{
    public interface IOpinionService
    {
        OpinionDetailsDto GetOpinionResponse(Opinion opinion);
        Task<List<Opinion>> GetPagedResponse(IQueryable<Opinion> opinions, int page, int pageSize);
        Task<Opinion> CreateOpinion(CreateOpinionDto opinionDto, string userId, int recipeId);
        Task UpdateOpinion(Opinion opinion, CreateOpinionDto opinionDto, int recipeId);
        Task DeleteOpinion(Opinion opinion, int recipeId);
        Task<bool> CheckForDuplicate(string userId, int recipeId);
    }

    public class OpinionService : IOpinionService
    {
        private IRecipeService recipeService;
        private RecipesDbContext context;

        public OpinionService(IRecipeService recipeService, RecipesDbContext context)
        {
            this.recipeService = recipeService;
            this.context = context;
        }

        public OpinionDetailsDto GetOpinionResponse(Opinion opinion)
        {
            return new OpinionDetailsDto
            {
                Id = opinion.Id,
                Rating = opinion.Rating,
                Comment = opinion.Comment,
                CreatedAt = opinion.CreatedAt,
                Author = new AuthorDto
                {
                    Id = opinion.UserId,
                    Username = opinion.User.UserName
                },
                RecipeId = opinion.RecipeId,
                RecipeName = opinion.Recipe == null
                    ? null
                    : opinion.Recipe.Name
            };
        }

        public async Task<List<Opinion>> GetPagedResponse(IQueryable<Opinion> opinions, int page, int pageSize)
        {
            return await opinions
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Opinion> CreateOpinion(CreateOpinionDto opinionDto, string userId, int recipeId)
        {
            Opinion opinion =  new Opinion
            {
                Rating = opinionDto.Rating,
                Comment = ProfanityFilter.FilterText(opinionDto.Comment),
                CreatedAt = DateTime.UtcNow,
                UserId = userId,
                RecipeId = recipeId
            };

            await context.Opinions.AddAsync(opinion);
            await context.SaveChangesAsync();
            await recipeService.UpdateRatingInfo(recipeId);

            return opinion;
        }

        public async Task UpdateOpinion(Opinion opinion, CreateOpinionDto opinionDto, int recipeId)
        {
            opinion.Rating = opinionDto.Rating;
            opinion.Comment = ProfanityFilter.FilterText(opinionDto.Comment);
     
            await context.SaveChangesAsync();
            await recipeService.UpdateRatingInfo(recipeId);
        }

        public async Task DeleteOpinion(Opinion opinion, int recipeId)
        {
            context.Opinions.Remove(opinion);
            await context.SaveChangesAsync();
            await recipeService.UpdateRatingInfo(recipeId);
        }

        public async Task<bool> CheckForDuplicate(string userId, int recipeId)
        {
            var opinion = await context.Opinions
                .FirstOrDefaultAsync(o => o.UserId == userId && o.RecipeId == recipeId);

            return opinion != null;
        }
    }
}
