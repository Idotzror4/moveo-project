using Microsoft.EntityFrameworkCore;
using MoveoBackend.Models;

namespace MoveoBackend.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserPreferences> UserPreferences { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<DailyContent> DailyContents { get; set; }
    }
}