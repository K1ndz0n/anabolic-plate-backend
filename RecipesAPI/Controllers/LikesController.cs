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
    public class LikesController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private ILikeService likeService;

        public LikesController(RecipesDbContext context, UserManager<AppUser> userManager, ILikeService likeService)
        {
            this.context = context;
            this.userManager = userManager;
            this.likeService = likeService;
        }

        [HttpGet("{recipeId}"), Authorize]
        public async Task<ActionResult<bool>> IsLiked(int recipeId)
        {
            var user = await userManager.GetUserAsync(User);
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            var like = await context.Likes.FirstOrDefaultAsync(
                l => l.RecipeId == recipeId && l.UserId == user.Id);

            return Ok(like != null);
        }

        [HttpPost("{recipeId}"), Authorize]
        public async Task<ActionResult<LikeDto>> AddLike(int recipeId)
        {
            var user = await userManager.GetUserAsync(User);
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            if (await likeService.CheckForDuplicate(user.Id, recipeId))
                return BadRequest("Przepis jest już zapisany");

            Like like = await likeService.CreateLike(user, recipeId);

            return Ok(new LikeDto
            {
                UserId = like.UserId,
                RecipeId = like.RecipeId
            });
        }

        [HttpDelete("{recipeId}"), Authorize]
        public async Task<IActionResult> DeleteLike(int recipeId)
        {
            var user = await userManager.GetUserAsync(User);
            var like = await context.Likes
                .FirstOrDefaultAsync(l => l.UserId == user.Id
                && l.RecipeId == recipeId);
            
            if (like == null)
                return NotFound();

            await likeService.DeleteLike(like);

            return NoContent();
        }
    }
}
