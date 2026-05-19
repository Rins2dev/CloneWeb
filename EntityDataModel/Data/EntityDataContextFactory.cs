using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EntityDataModel.Data
{
    // Used by "dotnet ef migrations add" and "dotnet ef database update" CLI tools.
    // Not used at runtime — runtime uses DI-injected DbContextOptions from Startup.cs.
    public class EntityDataContextFactory : IDesignTimeDbContextFactory<EntityDataContext>
    {
        public EntityDataContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "CloneWeb"))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<EntityDataContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new EntityDataContext(optionsBuilder.Options);
        }
    }
}
