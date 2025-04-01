using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO;

namespace MagazynLaptopowWPF.Models
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Pełna ścieżka do pliku bazy danych
            string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "MagazynDb.sqlite");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder
                .UseSqlite($"Data Source={dbPath}")
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}