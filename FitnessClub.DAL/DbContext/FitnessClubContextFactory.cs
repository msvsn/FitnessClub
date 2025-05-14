using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace FitnessClub.DAL
{
    public class FitnessClubContextFactory : IDesignTimeDbContextFactory<FitnessClubContext>
    {
        public FitnessClubContext CreateDbContext(string[] args)
        {
            string basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "FitnessClub.Web"));

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(basePath) 
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) 
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .Build();

            var builder = new DbContextOptionsBuilder<FitnessClubContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Не знайдено рядок підключення 'DefaultConnection' в appsettings.json. Пошук шляху: {Path.Combine(basePath, "appsettings.json")}");
            }

            builder.UseSqlite(connectionString);

            return new FitnessClubContext(builder.Options);
        }
    }
} 