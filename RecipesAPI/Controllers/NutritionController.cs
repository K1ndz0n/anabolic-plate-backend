using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;
using RecipesAPI.Services;

namespace RecipesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NutritionController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private INutritionService nutritionService;

        public NutritionController(RecipesDbContext context, UserManager<AppUser> userManager, INutritionService nutritionService)
        {
            this.context = context;
            this.userManager = userManager;
            this.nutritionService = nutritionService;
        }


        [HttpGet("/api/Recipes/{recipeId}/Nutrition")]
        public async Task<ActionResult<NutritionDetailsDto>> GetRecipeNutrition(int recipeId)
        {
            var nutrition = await context.Nutrition.FirstOrDefaultAsync(n => n.RecipeId == recipeId);
            if (nutrition == null)
                return NotFound();

            return Ok(nutritionService.GetNutritionResponse(nutrition));
        }

        [HttpPost("/api/Recipes/{recipeId}/Nutrition/add"), Authorize]
        public async Task<ActionResult<NutritionDetailsDto>> AddNutrition(int recipeId, CreateNutritionDto nutritionDto)
        {
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            if (nutritionDto == null)
                return BadRequest();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            if (await nutritionService.CheckForDuplicate(recipeId))
                return BadRequest("Informacja żywieniowa już istnieje");

            Nutrition nutrition = await nutritionService.CreateNutrition(nutritionDto, recipeId);

            return CreatedAtAction(
                nameof(GetRecipeNutrition),
                new { recipeId = recipeId },
                nutritionService.GetNutritionResponse(nutrition)
            );
        }

        [HttpPut("/api/Recipes/{recipeId}/Nutrition/edit"), Authorize]
        public async Task<ActionResult<NutritionDetailsDto>> EditNutririon(int recipeId, CreateNutritionDto nutritionDto)
        {
            var recipe = await context.Recipes
                .Include(r => r.Nutrition)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.Nutrition == null)
                return NotFound();

            if (nutritionDto == null)
                return BadRequest();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            var nutrition = recipe.Nutrition;
            await nutritionService.UpdateNutrition(nutritionDto, recipe.Nutrition);

            return Ok(nutritionService.GetNutritionResponse(nutrition));
        }

        [HttpDelete("/api/Recipes/{recipeId}/Nutrition/delete"), Authorize]
        public async Task<IActionResult> DeleteNutrition(int recipeId)
        {
            var recipe = await context.Recipes
                .Include(r => r.Nutrition)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null || recipe.Nutrition == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            await nutritionService.DeleteNutrition(recipe.Nutrition);

            return NoContent();
        }
    }
}
