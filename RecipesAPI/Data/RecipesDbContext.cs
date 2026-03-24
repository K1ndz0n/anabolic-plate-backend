using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RecipesAPI.Models;
using System.Reflection.Emit;

namespace RecipesAPI.Data
{
    public class RecipesDbContext : IdentityDbContext<AppUser>
    {
        public RecipesDbContext(DbContextOptions<RecipesDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Opinion>()
                .HasOne(o => o.Recipe)
                .WithMany(r => r.Opinions)
                .HasForeignKey(o => o.RecipeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Opinion>()
                .HasOne(o => o.User)
                .WithMany(u => u.Opinions)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Opinion>()
                .HasIndex(o => new { o.UserId, o.RecipeId })
                .IsUnique();

            builder.Entity<Follow>()
                .HasOne(f => f.User)
                .WithMany(u => u.Follows)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Follow>()
                .HasIndex(f => new { f.UserId, f.FollowedUserId })
                .IsUnique();

            builder.Entity<Like>()
                .HasIndex(f => new { f.UserId, f.RecipeId })
                .IsUnique();

            builder.Entity<AppUser>()
               .Property(u => u.FollowAmount)
               .HasDefaultValue(0);

            builder.Entity<Like>()
                .HasOne(l => l.Recipe)
                .WithMany(r => r.Likes)
                .HasForeignKey(l => l.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Like>()
                .HasOne(l => l.User)
                .WithMany(u => u.Likes)
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Opinion> Opinions { get; set; }
        public DbSet<Follow> Follows { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Nutrition> Nutrition { get; set; }
    }
}
