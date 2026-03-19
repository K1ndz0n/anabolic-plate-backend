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
    [Route("api/me")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private IRecipeService recipeService;
        private IOpinionService opinionService;

        public UserController(RecipesDbContext context, UserManager<AppUser> userManager,
            IOpinionService opinionService, IRecipeService recipeService)
        {
            this.context = context;
            this.userManager = userManager;
            this.opinionService = opinionService;
            this.recipeService = recipeService;
        }

        [HttpGet("/api/me"), Authorize]
        public async Task<ActionResult<AuthorDto>> GetMe()
        {
            var user = await userManager.GetUserAsync(User);

            return new AuthorDto
            {
                Id = user.Id,
                Username = user.UserName,
                FollowAmount = user.FollowAmount
            };
        }

        [HttpGet("/api/user/{username}")]
        public async Task<ActionResult<AuthorDto>> GetUserByUsername(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound();

            return new AuthorDto
            {
                Id = user.Id,
                Username = user.UserName,
                FollowAmount = user.FollowAmount
            };
        }

        [HttpGet("Recipes"), Authorize]
        public async Task<ActionResult<List<RecipeDetailsDto>>> GetMyRecipes(int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);
            var recipes = recipeService.GetRecipesQueryable()
                .Where(r => r.User == user)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return Ok(recipes.Select(r => recipeService.GetRecipeResponse(r)));
        }

        [HttpGet("RecipeThumbnails"), Authorize]
        public async Task<ActionResult<PagedResponse<RecipeThumbnailDto>>> GetMyRecipeThumbnails(
            [FromQuery] Filter filter, int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);

            var query = context.Recipes
                .AsNoTracking()
                .Where(r => r.UserId == user.Id);

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

        [HttpGet("Opinions"), Authorize]
        public async Task<ActionResult<PagedResponse<OpinionDetailsDto>>> GetMyOpinions(int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);
            var query = context.Opinions
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .Where(o => o.User == user);

            var totalCount = await query.CountAsync();

            var opinions = await opinionService.GetPagedResponse(query, page, pageSize);

            var response = new PagedResponse<OpinionDetailsDto>
            {
                Items = opinions.Select(o => opinionService.GetOpinionResponse(o)).ToList(),
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return Ok(response);
        }

        [HttpGet("/api/Recipes/{recipeId}/MyOpinion"), Authorize]
        public async Task<ActionResult<OpinionDetailsDto>> GetMyOpinion(int recipeId)
        {
            var user = await userManager.GetUserAsync(User);

            var recipe = await context.Recipes
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
                return NotFound("Nie znaleziono przepisu");

            var opinion = await context.Opinions.FirstOrDefaultAsync(
                o => o.RecipeId == recipeId
                && o.UserId == user.Id);

            if (opinion == null)
                return NotFound("Nie znaleziono opinii");

            return Ok(opinionService.GetOpinionResponse(opinion));
        }

        [HttpGet("Liked"), Authorize]
        public async Task<ActionResult<PagedResponse<RecipeThumbnailDto>>> GetLikedRecipeThumbnails(
            [FromQuery] Filter filter, int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);

            var query = context.Recipes
                .AsNoTracking()
                .Where(r => context.Likes
                    .Any(l => l.UserId == user.Id && l.RecipeId == r.Id));

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

        [HttpGet("Followed"), Authorize]
        public async Task<ActionResult<PagedResponse<RecipeThumbnailDto>>> GetFollowedRecipeThumbnails(
            [FromQuery] Filter filter, int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);
            var query = context.Recipes
                .AsNoTracking()
                .Where(r => context.Follows
                    .Any(f => f.UserId == user.Id && f.FollowedUserId == r.UserId));

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