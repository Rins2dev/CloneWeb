using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EnityModel.Data
{
    public partial class EntityDataContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfigurationRoot configuration = new ConfigurationBuilder()
                               .SetBasePath(Directory.GetCurrentDirectory())
                               .AddJsonFile("appsettings.json")
                               .Build();
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString, sqlServerOptions => sqlServerOptions.CommandTimeout(6000));
            }
        }
    }
}

namespace EntityDataModel.Data
{
    public partial class EntityDataContext
    {
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // PasswordHasher produces hashes ~84 chars; override the auto-generated HasMaxLength(50)
            modelBuilder.Entity<EntityDataModel.Models.User>(entity =>
            {
                entity.Property(e => e.Password).HasMaxLength(256);
            });
        }
    }
}
