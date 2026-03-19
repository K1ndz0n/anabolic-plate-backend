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
    public class OpinionsController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private IOpinionService opinionService;

        public OpinionsController(RecipesDbContext context, UserManager<AppUser> userManager, IOpinionService opinionService)
        {
            this.context = context;
            this.userManager = userManager;
            this.opinionService = opinionService;
        }

        [HttpGet("get/{recipeId}")]
        public async Task<ActionResult<PagedResponse<OpinionDetailsDto>>> GetRecipeOpinions(
            int recipeId, int page = 1, int pageSize = 10)
        {
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            var query = context.Opinions
                .AsNoTracking()
                .Include(o => o.User)
                .Where(o => o.RecipeId == recipe.Id)
                .OrderByDescending(o => o.Id);

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

        [HttpGet("get/{recipeId}/WhenLogged"), Authorize]
        public async Task<ActionResult<PagedResponse<OpinionDetailsDto>>> GetRecipeOpinionsWhenLogged(
            int recipeId, int page = 1, int pageSize = 10)
        {
            var user = await userManager.GetUserAsync(User);

            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            var query = context.Opinions
                .AsNoTracking()
                .Include(o => o.User)
                .Where(o => o.RecipeId == recipe.Id
                && o.UserId != user.Id);

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

        [HttpGet("{id}")]
        public async Task<ActionResult<OpinionDetailsDto>> GetOpinionDetails(int id)
        {
            var opinion = await context.Opinions
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opinion == null) 
                return NotFound();

            return Ok(opinionService.GetOpinionResponse(opinion));
        }

        [HttpPost("add/{recipeId}"), Authorize]
        public async Task<ActionResult<OpinionDetailsDto>> AddOpinion(int recipeId, CreateOpinionDto opinionDto)
        {
            var recipe = await context.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId);
            if (recipe == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (recipe.UserId == user.Id)
                return BadRequest("Nie można zostawić opinii pod swoim przepisem");

            if (await opinionService.CheckForDuplicate(user.Id, recipeId))
                return BadRequest("Można zostawić tylko 1 opinię");

            Opinion opinion = await opinionService.CreateOpinion(opinionDto, user.Id, recipeId);

            return CreatedAtAction(
                nameof(GetOpinionDetails),
                new { id =  opinion.Id },
                opinionService.GetOpinionResponse(opinion)
            );
        }

        [HttpPut("edit/{recipeId}"), Authorize]
        public async Task<ActionResult<OpinionDetailsDto>> EditOpinion(int recipeId, CreateOpinionDto opinionDto)
        {
            var user = await userManager.GetUserAsync(User);
            var opinion = await context.Opinions
                .FirstOrDefaultAsync(o =>  o.RecipeId == recipeId && o.UserId == user.Id);

            if (opinion == null)
                return NotFound();

            await opinionService.UpdateOpinion(opinion, opinionDto, opinion.RecipeId);

            return Ok(opinionService.GetOpinionResponse(opinion));
        }

        [HttpDelete("delete/{recipeId}"), Authorize]
        public async Task<IActionResult> DeleteOpinion(int recipeId)
        {
            var user = await userManager.GetUserAsync(User);
            var opinion = await context.Opinions
                .FirstOrDefaultAsync(o => o.RecipeId == recipeId && o.UserId == user.Id);

            if (opinion == null)
                return NotFound();

            await opinionService.DeleteOpinion(opinion, opinion.RecipeId);

            return NoContent();
        }

        [HttpGet("/api/Users/{username}/Opinions")]
        public async Task<ActionResult<PagedResponse<OpinionDetailsDto>>> GetOpinionsByUsername(
            string username, int page = 1, int pageSize = 10)
        {
            var query = context.Opinions
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.Recipe)
                .Where(o => o.User.UserName == username);

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
    }
}
