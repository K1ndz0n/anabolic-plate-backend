using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;
using RecipesAPI.Services.Utils;
using System.ComponentModel.DataAnnotations;

namespace RecipesAPI.Services
{
    public interface IRecipeService
    {
        IQueryable<Recipe> GetRecipesQueryable();
        RecipeDetailsDto GetRecipeResponse(Recipe recipe);
        Task<Recipe> CreateRecipe(CreateRecipeDto recipeDto, string userId);
        Task UpdateRecipe(Recipe recipe, CreateRecipeDto recipeDto);
        RecipeThumbnailDto GetRecipeThumbnail(Recipe recipe);
        Task UpdateRatingInfo(int recipeId);
        Task<IQueryable<Recipe>> GetFilteredResponse(IQueryable<Recipe> recipes, Filter filter);      
    }

    public class RecipeService : IRecipeService
    {
        private RecipesDbContext context;

        public RecipeService(RecipesDbContext context)
        {
            this.context = context;
        }

        public IQueryable<Recipe> GetRecipesQueryable()
        {
            return context.Recipes
                .Include(r => r.User)
                .Include(r => r.Nutrition)
                .Include(r => r.Ingredients)
                .Include(r => r.Photos);
        }

        public RecipeDetailsDto GetRecipeResponse(Recipe recipe)
        {
            return new RecipeDetailsDto
            {
                Id = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Steps = recipe.Steps,
                Rating = recipe.Rating,
                OpinionAmount = recipe.OpinionAmount,
                Author = new AuthorDto
                {
                    Id = recipe.User.Id,
                    Username = recipe.User.UserName,
                    FollowAmount = recipe.User.FollowAmount
                },
                Nutrition = recipe.Nutrition == null ? null : new NutritionDetailsDto
                {
                    Protein = recipe.Nutrition.Protein,
                    Carbs = recipe.Nutrition.Carbs,
                    Fat = recipe.Nutrition.Fat,
                    Kcal = recipe.Nutrition.Kcal
                },
                Ingredients = recipe.Ingredients
                    .Select(i => new IngredientDto
                    {
                        Name = i.Name,
                        Amount = i.Amount,
                        Unit = i.Unit
                    }).ToList(),
                PhotoUrls = recipe.Photos
                    .Select(p => new PhotoDto
                    {
                        Id = p.Id,
                        Path = p.Path
                    }).ToList()
            };
        }

        public async Task<Recipe> CreateRecipe(CreateRecipeDto recipeDto, string userId)
        {
            if (ProfanityFilter.RecipeContainsProfanity(recipeDto))
                throw new ValidationException("Przepis zawiera nieodpowiednie słownictwo.");

            Recipe recipe =  new Recipe
            {
                Name = recipeDto.Name,
                Description = recipeDto.Description,
                Steps = recipeDto.Steps,
                Rating = 0f,
                OpinionAmount = 0,
                UserId = userId,
                Ingredients = recipeDto.Ingredients
                    .Select(i => new Ingredient
                    {
                        Name = i.Name,
                        Amount = i.Amount,
                        Unit = i.Unit
                    }).ToList()
            };

            await context.Recipes.AddAsync(recipe);
            await context.SaveChangesAsync();

            return recipe;
        }

        public async Task UpdateRecipe(Recipe recipe, CreateRecipeDto recipeDto)
        {
            if (ProfanityFilter.RecipeContainsProfanity(recipeDto))
                throw new ValidationException("Przepis zawiera nieodpowiednie słownictwo.");

            recipe.Name = recipeDto.Name;
            recipe.Description = recipeDto.Description;
            recipe.Steps = recipeDto.Steps;

            recipe.Ingredients.Clear();
            recipe.Ingredients = recipeDto.Ingredients
                .Select(i => new Ingredient
                {
                    Name = i.Name,
                    Amount = i.Amount,
                    Unit = i.Unit
                }).ToList();

            await context.SaveChangesAsync();
        }

        public RecipeThumbnailDto GetRecipeThumbnail(Recipe recipe)
        {
            return new RecipeThumbnailDto
            {
                RecipeId = recipe.Id,
                Name = recipe.Name,
                Description = recipe.Description,
                Rating = recipe.Rating,
                OpinionAmount = recipe.OpinionAmount,
                Author = new AuthorDto
                {
                    Id = recipe.User.Id,
                    Username = recipe.User.UserName,
                    FollowAmount = recipe.User.FollowAmount
                },
                ThumbnailPhotoUrl = recipe.Photos.Count > 0
                    ? recipe.Photos[0].Path : null
            };
        }

        public async Task UpdateRatingInfo(int recipeId)
        {
            var recipe = await context.Recipes
                .Include(r => r.Opinions)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
                return;

            recipe.OpinionAmount = recipe.Opinions.Count;

            recipe.Rating = recipe.Opinions.Count > 0
                ? recipe.Rating = (float)recipe.Opinions.Average(o => o.Rating)
                : recipe.Rating = 0;
  
            await context.SaveChangesAsync();
        }

        public async Task<IQueryable<Recipe>> GetFilteredResponse(
            IQueryable<Recipe> recipes, Filter filter)
        {
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                filter.Search = filter.Search.ToLower();
                recipes = recipes.Where(r =>
                    r.Name.ToLower().Contains(filter.Search) ||
                    r.Description != null && r.Description.ToLower().Contains(filter.Search) ||
                    r.Ingredients.Any(i => i.Name.ToLower().Contains(filter.Search)) ||
                    r.User.UserName.ToLower().Contains(filter.Search)
                );
            }

            recipes = recipes.Where(r => r.Rating >= filter.MinRating && r.Rating <= filter.MaxRating)
                .Where(r => r.OpinionAmount >= filter.MinOpinionCount && r.OpinionAmount <= filter.MaxOpinionCount);

            if (filter.HasNutrition)
            {
                recipes = recipes.Where(r =>
                    (r.Nutrition != null) &&
                    r.Nutrition.Kcal >= filter.MinKcal && r.Nutrition.Kcal <= filter.MaxKcal &&
                    r.Nutrition.Protein >= filter.MinProtein && r.Nutrition.Protein <= filter.MaxProtein &&
                    r.Nutrition.Carbs >= filter.MinCarbs && r.Nutrition.Carbs <= filter.MaxCarbs &&
                    r.Nutrition.Fat >= filter.MinFat && r.Nutrition.Fat <= filter.MaxFat
                );
            }

            recipes = filter.OrderBy.ToLower() switch
            {
                "recipeid" => filter.Order == "asc"
                    ? recipes.OrderBy(r => r.Id)
                    : recipes.OrderByDescending(r => r.Id),

                "rating" => filter.Order == "asc"
                    ? recipes.OrderBy(r => r.Rating)
                    : recipes.OrderByDescending(r => r.Rating),

                "opinionamount" => filter.Order == "asc"
                    ? recipes.OrderBy(r => r.OpinionAmount)
                    : recipes.OrderByDescending(r => r.OpinionAmount),

                _ => recipes.OrderByDescending(r => r.Id)
            };

            return recipes
                .Include(r => r.User)
                .Include(r => r.Photos)
                .Include(r => r.Opinions);       
        }
    }
}
