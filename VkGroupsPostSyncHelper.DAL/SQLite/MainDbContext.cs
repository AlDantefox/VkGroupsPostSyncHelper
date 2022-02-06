using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VkGroupsPostSyncHelper.DAL.SQLite
{
    public class MainDbContext : DbContext
    {
        public DbSet<VkGroupPost> VkGroupPosts { get; set; }
        public DbSet<VkPostImage> VkPostImages { get; set; }

        public string DbPath { get; private set; }

        private IConfiguration _config;
        public MainDbContext(IConfiguration config)
        {
            _config = config;
            Database.EnsureCreated();
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            DbPath = _config[nameof(DbPath)];
            options.UseSqlite($"Data Source={DbPath}");
        }
    }
}

