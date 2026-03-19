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
    public class FollowsController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private IFollowService followService;

        public FollowsController(RecipesDbContext context, UserManager<AppUser> userManager, IFollowService followService)
        {
            this.context = context;
            this.userManager = userManager;
            this.followService = followService;
        }

        [HttpGet("{username}"), Authorize]
        public async Task<ActionResult<bool>> IsFollowed(string username)
        {
            var user = await userManager.GetUserAsync(User);
            var userToFollow = await userManager.FindByNameAsync(username);
            if (userToFollow == null)
                return NotFound();

            var follow = await context.Follows.FirstOrDefaultAsync(
                f => f.UserId == user.Id && f.FollowedUserId == userToFollow.Id);

            return Ok(follow != null);
        }

        [HttpPost("{username}"), Authorize]
        public async Task<ActionResult<FollowDto>> AddFollow(string username)
        {
            var user = await userManager.GetUserAsync(User);
            var userToFollow = await userManager.FindByNameAsync(username);
            if (userToFollow == null)
                return NotFound();

            if (user == userToFollow)
                return BadRequest("Nie można obserwować swojego konta");

            if (await followService.CheckForDuplicate(user.Id, userToFollow.Id))
                return BadRequest("Użytkownik jest już obserwowany");

            Follow follow = await followService.CreateFollow(user, userToFollow);

            return Ok(new FollowDto
            {
                Id = follow.Id,
                UserId = follow.UserId,
                FollowedUserId = follow.FollowedUserId
            });
        }

        [HttpDelete("{username}"), Authorize]
        public async Task<IActionResult> DeleteFollow(string username)
        {
            var user = await userManager.GetUserAsync(User);
            var userToUnfollow = await userManager.FindByNameAsync(username);
            if (userToUnfollow == null)
                return NotFound("Nie znaleziono użytkownika");

            var follow = await context.Follows
                .FirstOrDefaultAsync(f => f.UserId == user.Id 
                && f.FollowedUserId == userToUnfollow.Id);

            if (follow == null)
                return NotFound();

            await followService.DeleteFollow(follow, userToUnfollow);

            return NoContent();
        }
    }
}
