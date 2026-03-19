using RecipesAPI.Data;
using RecipesAPI.DTOs;
using RecipesAPI.Models;

namespace RecipesAPI.Services
{
    public interface IPhotoService
    {
        PhotoDto GetPhotoResponse(Photo photo);
        Task <Photo> CreatePhoto(IFormFile file, int recipeId);
        Task DeletePhoto(Photo photo);
    }
    public class PhotoService : IPhotoService
    {
        private RecipesDbContext context;

        public PhotoService(RecipesDbContext context)
        {
            this.context = context;
        }

        public PhotoDto GetPhotoResponse(Photo photo)
        {
            return new PhotoDto
            {
                Id = photo.Id,
                Path = photo.Path
            };
        }

        public async Task<Photo> CreatePhoto(IFormFile file, int recipeId)
        {
            var uploadsDir = Path.Combine("wwwroot", "uploads", "photos");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            Photo photo = new Photo
            {
                Path = "/uploads/photos/" + fileName,
                RecipeId = recipeId
            };

            await context.Photos.AddAsync(photo);
            await context.SaveChangesAsync();

            return photo;
        }

        public async Task DeletePhoto(Photo photo)
        {
            var filePath = Path.Combine("wwwroot", photo.Path.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            context.Photos.Remove(photo);
            await context.SaveChangesAsync();
        }
    }
}
