using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TOH.Server.Data
{
    public class GameDbContext : DbContext, IDataProtectionKeyContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options)
            : base(options)
        {
        }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<PlayerUnit> PlayerUnits { get; set; }
        public DbSet<PlayerSession> PlayerSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("uuid-ossp");

            builder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasAlternateKey(e => e.Username);
            });

            builder.Entity<PlayerUnit>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Player).WithMany(e => e.Units).HasForeignKey(e => e.PlayerId); ;
            });

            builder.Entity<PlayerSession>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.Player).WithMany().HasForeignKey(e => e.PlayerId); ;
            });
        }
    }
}
