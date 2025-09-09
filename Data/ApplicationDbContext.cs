using Microsoft.EntityFrameworkCore;
using SadiclarasanWeb.Models;

namespace SadiclarasanWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<AdminUser> AdminUsers { get; set; }
        public DbSet<SeoSettings> SeoSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // BlogPost konfigürasyonu
            builder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Summary).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // Contact konfigürasyonu
            builder.Entity<Contact>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // AdminUser konfigürasyonu
            builder.Entity<AdminUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // SeoSettings konfigürasyonu
            builder.Entity<SeoSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.SiteTitle).IsRequired().HasMaxLength(100);
                entity.Property(e => e.SiteDescription).HasMaxLength(300);
                entity.Property(e => e.SiteUrl).HasMaxLength(100);
                entity.Property(e => e.UpdatedDate).HasDefaultValueSql("GETDATE()");
            });
        }
    }
}
