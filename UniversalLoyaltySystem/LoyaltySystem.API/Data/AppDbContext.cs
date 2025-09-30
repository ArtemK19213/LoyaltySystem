using Microsoft.EntityFrameworkCore;
using LoyaltySystem.API.Models.Entities; 

namespace LoyaltySystem.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opt) : base(opt) { }

        public DbSet<LoyaltyUser> Users => Set<LoyaltyUser>();
        public DbSet<LoyaltyCard> Cards => Set<LoyaltyCard>();


        protected override void OnModelCreating(ModelBuilder b)
        {

        }
    }

}
