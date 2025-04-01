using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.IO;

namespace MagazynLaptopowWPF.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Laptop> Laptopy { get; set; } // Tabela "Laptopy" w bazie

        // Domyślny konstruktor
        public AppDbContext()
        {
        }

        // Konstruktor przyjmujący opcje, używany do wstrzykiwania konfiguracji
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Jeśli opcje nie zostały jeszcze skonfigurowane (np. przy użyciu domyślnego konstruktora)
            if (!optionsBuilder.IsConfigured)
            {
                // Ścieżka do pliku bazy danych SQLite - używamy bezpiecznej domyślnej ścieżki
                string dbPath = Path.Combine(Directory.GetCurrentDirectory(), "MagazynDb.sqlite");

                // Upewnij się, że katalog istnieje
                string? directory = Path.GetDirectoryName(dbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Konfiguracja połączenia
                optionsBuilder
                    .UseSqlite($"Data Source={dbPath}")
                    // Ignoruj ostrzeżenie o zmianach w modelu
                    .ConfigureWarnings(warnings =>
                    {
                        warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                    })
                    // Opcjonalnie: włącz logowanie danych wrażliwych, aby zobaczyć więcej szczegółów błędu
                    .EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja modelu
            modelBuilder.Entity<Laptop>().HasIndex(l => l.Marka);
            modelBuilder.Entity<Laptop>().HasIndex(l => l.Model);

            // Upewnij się, że wszystkie wymagane kolumny mają domyślne wartości
            modelBuilder.Entity<Laptop>()
                .Property(l => l.Ilosc)
                .HasDefaultValue(0);

            modelBuilder.Entity<Laptop>()
                .Property(l => l.Marka)
                .HasDefaultValue("");

            modelBuilder.Entity<Laptop>()
                .Property(l => l.Model)
                .HasDefaultValue("");
        }
    }
}