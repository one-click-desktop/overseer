using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OneClickDesktop.Overseer.Entities;

namespace OneClickDesktop.Overseer.Helpers
{
    public class TestDataContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        private readonly IConfiguration Configuration;

        public TestDataContext(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // in memory database used for simplicity, change to a real db for production applications
            options.UseInMemoryDatabase("TestDb");
        }
    }
}
