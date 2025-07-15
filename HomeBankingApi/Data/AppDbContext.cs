using HomeBankingApi.models;
using Microsoft.EntityFrameworkCore;

namespace HomeBankingApi.Data
{
    public class HomeBankingContext : DbContext
    {
        public HomeBankingContext(DbContextOptions<HomeBankingContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
