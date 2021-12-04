using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkGroupsPostSyncHelper.SQLite
{
    public class MainDbContext : DbContext
    {
        public DbSet<VkGroupPost> VkGroupPosts { get; set; }
        public DbSet<VkPostImage> VkPostImages { get; set; }

        public string DbPath { get; private set; }

        public MainDbContext()
        {
            Database.EnsureCreated();
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            DbPath = $"vk.db";
            options.UseSqlite($"Data Source={DbPath}");
        }
    }
}

