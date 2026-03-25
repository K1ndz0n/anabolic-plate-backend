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
    public class PhotosController : ControllerBase
    {
        private readonly RecipesDbContext context;
        private readonly UserManager<AppUser> userManager;
        private IPhotoService photoService;

        public PhotosController(RecipesDbContext context, UserManager<AppUser> userManager, IPhotoService photoService)
        {
            this.context = context;
            this.userManager = userManager;
            this.photoService = photoService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PhotoDto>>> Get()
        {
            var photos = await context.Photos
                .AsNoTracking()
                .ToListAsync();

            return Ok(photos.Select(p => photoService.GetPhotoResponse(p)));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhotoDto>> GetPhoto(int id)
        {
            var photo = await context.Photos
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null)
                return NotFound();

            return Ok(photoService.GetPhotoResponse(photo));
        }

        [HttpHead("/api/Recipes/{recipeId}/Photos")]
        [HttpGet("/api/Recipes/{recipeId}/Photos")]
        public async Task<ActionResult<List<PhotoDto>>> GetRecipePhotos(int recipeId)
        {
            var recipe = await context.Recipes
                .Include(r => r.Photos)
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
                return NotFound();

            return Ok(recipe.Photos.Select(p => photoService.GetPhotoResponse(p)).ToList());
        }

        [HttpPost("/api/Recipes/{recipeId}/Photos/add"), Authorize]
        public async Task<ActionResult<PhotoDto>> AddPhoto(int recipeId, IFormFile file)
        {
            var recipe = await context.Recipes
                .FirstOrDefaultAsync(r => r.Id == recipeId);

            if (recipe == null)
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (user.Id != recipe.UserId)
                return Forbid();

            var photoCount = await context.Photos.CountAsync(p => p.RecipeId == recipeId);
            if (photoCount > 10)
                return BadRequest("Max. ilość zdjęć to 10");

            if (file == null)
                return BadRequest("Brak pliku.");


            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Nieobsługiwany typ pliku.");

            var maxFileSize = 5 * 1024 * 1024; // 5 MB
            if (file.Length > maxFileSize)
                return BadRequest("Plik jest za duży.");

            Photo photo = await photoService.CreatePhoto(file, recipeId);
            
            return CreatedAtAction(
                nameof(GetPhoto),
                new { id = photo.Id },
                photoService.GetPhotoResponse(photo)
            );
        }

        [HttpDelete("delete/{id}"), Authorize]
        public async Task<IActionResult> DeletePhoto(int id)
        {
            var photo = await context.Photos
                .Include(p => p.Recipe)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (photo == null) 
                return NotFound();

            var user = await userManager.GetUserAsync(User);
            if (photo.Recipe.UserId != user.Id)
                return Forbid();

            await photoService.DeletePhoto(photo);
            
            return NoContent();
        }
    }
}
