using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;

namespace RecipesAPI.Services
{
    public interface INutritionService
    {
        NutritionDetailsDto GetNutritionResponse(Nutrition nutrition);
        float CalculateKcal(CreateNutritionDto nutritionDto);
        Task<Nutrition> CreateNutrition(CreateNutritionDto nutritionDto, int recipeId);
        Task UpdateNutrition(CreateNutritionDto nutritionDto, Nutrition nutrition);
        Task DeleteNutrition(Nutrition nutrition);
        Task<bool> CheckForDuplicate(int recipeId);
    }

    public class NutritionService : INutritionService
    {
        private RecipesDbContext context;

        public NutritionService(RecipesDbContext context)
        {
            this.context = context;
        }

        public NutritionDetailsDto GetNutritionResponse(Nutrition nutrition)
        {
            return new NutritionDetailsDto
            {
                Protein = nutrition.Protein,
                Carbs = nutrition.Carbs,
                Fat = nutrition.Fat,
                Kcal = nutrition.Kcal
            };
        }

        public float CalculateKcal(CreateNutritionDto nutritionDto)
        {
            return (4 * nutritionDto.Protein)
                + (4 * nutritionDto.Carbs)
                + (9 * nutritionDto.Fat);
        }

        public async Task<Nutrition> CreateNutrition(CreateNutritionDto nutritionDto, int recipeId)
        {
            Nutrition nutrition = new Nutrition
            {
                Protein = nutritionDto.Protein,
                Carbs = nutritionDto.Carbs,
                Fat = nutritionDto.Fat,
                Kcal = CalculateKcal(nutritionDto),
                RecipeId = recipeId
            };

            await context.Nutrition.AddAsync(nutrition);
            await context.SaveChangesAsync();

            return nutrition;
        }

        public async Task UpdateNutrition(CreateNutritionDto nutritionDto, Nutrition nutrition)
        {
            nutrition.Protein = nutritionDto.Protein;
            nutrition.Carbs = nutritionDto.Carbs;
            nutrition.Fat = nutritionDto.Fat;
            nutrition.Kcal = CalculateKcal(nutritionDto);

            await context.SaveChangesAsync();
        }

        public async Task DeleteNutrition(Nutrition nutrition)
        {
            context.Nutrition.Remove(nutrition);
            await context.SaveChangesAsync();
        }

        public async Task<bool> CheckForDuplicate(int recipeId)
        {
            var nutrition = await context.Nutrition
                .FirstOrDefaultAsync(n => n.RecipeId == recipeId);

            return nutrition != null;
        }
    }
}
