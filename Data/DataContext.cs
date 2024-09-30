namespace Redm_backend.Data
{
    using Microsoft.EntityFrameworkCore;
    using Redm_backend.Models;

    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        // This is what EF uses for mapping and querying models
        public DbSet<User> Users => Set<User>();

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public DbSet<Symptom> Symptoms => Set<Symptom>();

        public DbSet<PostCategory> PostCategories => Set<PostCategory>();

        public DbSet<Post> Posts => Set<Post>();

        public DbSet<Story> Stories => Set<Story>();

        public DbSet<PeriodHistory> PeriodHistory => Set<PeriodHistory>();
    }
}