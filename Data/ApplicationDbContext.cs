using Document_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace Document_Management_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Models.Document> Documents => Set<Models.Document>();

        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Models.Document>()
                .Property(x => x.Status)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}