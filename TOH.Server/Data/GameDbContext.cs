using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TOH.Server.Data
{
    class GameDbContext : DbContext, IDataProtectionKeyContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");

            builder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(e => e.Username);
            });
        }
    }
}
