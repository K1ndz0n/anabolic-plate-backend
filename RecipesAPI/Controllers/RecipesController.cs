using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;
using RecipesAPI.Services;
using RecipesAPI.Services.Utils;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace RecipesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecipesController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private IRecipeService recipeService;

        public RecipesController(RecipesDbContext context, IRecipeService recipeService,
            UserManager<AppUser> userManager)
        {
            this.context = context;
            this.recipeService = recipeService;
            this.userManager = userManager;
        }

        [HttpGet("all")]
        public async Task<ActionResult<List<RecipeDetailsDto>>> Get(int page = 1, int pageSize = 10)
        {
            var recipes = recipeService.GetRecipesQueryable()
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return Ok(recipes.Select(r => recipeService.GetRecipeResponse(r)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDetailsDto>> GetRecipeDetails(int id)
        {
            var recipe = await recipeService.GetRecipesQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            return Ok(recipeService.GetRecipeResponse(recipe));
        }

        [HttpPost("add"), Authorize]
        public async Task<ActionResult<RecipeDetailsDto>> AddRecipe(CreateRecipeDto recipeDto)
        {
            if (recipeDto == null)
                return BadRequest();

            var user = await userManager.GetUserAsync(User);
            Recipe recipe = await recipeService.CreateRecipe(recipeDto, user.Id);

            return CreatedAtAction(
                nameof(GetRecipeDetails),
                new { id = recipe.Id },
                recipeService.GetRecipeResponse(recipe)
            );
        }

        [HttpPut("edit/{id}"), Authorize]
        public async Task<ActionResult<RecipeDetailsDto>> EditRecipe(int id, CreateRecipeDto recipeDto)
        {
            var recipe = await recipeService.GetRecipesQueryable()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            await recipeService.UpdateRecipe(recipe, recipeDto);

            return Ok(recipeService.GetRecipeResponse(recipe));
        }

        [HttpDelete("delete/{id}"), Authorize]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            var recipe = await recipeService.GetRecipesQueryable()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("thumbnails")]
        public async Task<ActionResult<PagedResponse<RecipeThumbnailDto>>> GetRecipeThumbnails(
            [FromQuery] Filter filter, int page = 1, int pageSize = 10)
        {
            var query = context.Recipes
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var filteredQuery = await recipeService.GetFilteredResponse(query, filter);
            var itemsCount = filteredQuery.Count();

            var recipes = await filteredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<RecipeThumbnailDto>
            {
                Items = recipes.Select(r => recipeService.GetRecipeThumbnail(r)).ToList(),
                Page = page,
                PageSize = pageSize,
                ItemsCount = itemsCount,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(itemsCount / (double)pageSize)
            };

            return Ok(response);
        }

        [HttpGet("/api/Users/{username}/thumbnails")]
        public async Task<ActionResult<PagedResponse<RecipeThumbnailDto>>> GetThumbnailsByUsername(
            string username, [FromQuery] Filter filter, int page = 1, int pageSize = 10)
        {
            var query = context.Recipes
                .AsNoTracking()
                .Where(r => r.User.UserName == username);

            var totalCount = await query.CountAsync();
            var filteredQuery = await recipeService.GetFilteredResponse(query, filter);
            var itemsCount = filteredQuery.Count();

            var recipes = await filteredQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new PagedResponse<RecipeThumbnailDto>
            {
                Items = recipes.Select(r => recipeService.GetRecipeThumbnail(r)).ToList(),
                Page = page,
                PageSize = pageSize,
                ItemsCount = itemsCount,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(itemsCount / (double)pageSize)
            };

            return Ok(response);
        }
    }
}
