using Microsoft.EntityFrameworkCore;

namespace MagazynLaptopowWPF.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<Laptop> Laptopy { get; set; } // Tabela "Laptopy" w bazie

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Ścieżka do pliku bazy danych SQLite
            // Upewnij się, że aplikacja ma prawa do zapisu w tym miejscu
            optionsBuilder.UseSqlite("Data Source=MagazynDb.sqlite");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tutaj można dodać dodatkową konfigurację modelu, np. indeksy
            modelBuilder.Entity<Laptop>().HasIndex(l => l.Marka);
            modelBuilder.Entity<Laptop>().HasIndex(l => l.Model);

            // Można też dodać początkowe dane (seed data)
            // modelBuilder.Entity<Laptop>().HasData(
            //     new Laptop { Id = 1, Marka = "Dell", Model = "XPS 13", Ilosc = 5, SystemOperacyjny = "Windows 11", RozmiarEkranu = 13.4 },
            //     new Laptop { Id = 2, Marka = "Apple", Model = "MacBook Air M1", Ilosc = 3, SystemOperacyjny = "macOS Monterey", RozmiarEkranu = 13.3 }
            // );
        }
    }
}