using System;
using System.IO;
using System.Linq;
using System.Windows;
using MagazynLaptopowWPF.Models;
using MagazynLaptopowWPF.Views; // Dodaj using dla LoginWindow
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading.Tasks;

namespace MagazynLaptopowWPF
{
    public partial class App : Application
    {
        // Kontekst bazy danych dostępny globalnie (ale inicjowany raz)
        public static AppDbContext? DbContext { get; private set; }
        // Ścieżka do pliku bazy danych
        public static string DbPath { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "MagazynDb.sqlite");

        // Metoda uruchamiana przy starcie aplikacji
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Krok 1: Inicjalizacja bazy danych
            bool dbInitialized = await InitializeDatabaseAsync();
            if (!dbInitialized)
            {
                // Jeśli inicjalizacja się nie powiodła, zakończ aplikację
                Current.Shutdown(-1); // Zamknij z kodem błędu
                return;
            }

            // Krok 2: Uruchomienie okna logowania jako pierwszego
            var loginWindow = new LoginWindow();
            // Ustawienie MainWindow aplikacji na okno logowania, aby było ono "główne"
            // na czas procesu logowania. Po udanym logowaniu zostanie podmienione.
            this.MainWindow = loginWindow;
            loginWindow.Show();
            // Dalsza logika (otwarcie MainWindow po udanym logowaniu)
            // jest teraz realizowana wewnątrz LoginWindow i LoginViewModel.
        }

        // Prywatna metoda do inicjalizacji połączenia z bazą danych i jej utworzenia/migracji
        private async Task<bool> InitializeDatabaseAsync()
        {
            try
            {
                // Upewnij się, że katalog dla pliku bazy danych istnieje
                string? directory = Path.GetDirectoryName(DbPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Konfiguracja opcji dla Entity Framework Core i SQLite
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder
                    .UseSqlite($"Data Source={DbPath}") // Ustawienie źródła danych
                    .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)) // Ignoruj niektóre ostrzeżenia EF
                    .EnableSensitiveDataLogging(); // Włącz logowanie szczegółów (przydatne przy debugowaniu)

                // Utworzenie instancji kontekstu bazy danych
                DbContext = new AppDbContext(optionsBuilder.Options);

                // Upewnij się, że baza danych istnieje (utwórz ją lub zastosuj migracje, jeśli ich używasz)
                // W tym przypadku EnsureCreated() utworzy bazę, jeśli nie istnieje, na podstawie modelu.
                await DbContext.Database.EnsureCreatedAsync();

                // Opcjonalnie: Dodaj przykładowe dane, jeśli tabela Laptopy jest pusta
                // To jest przydatne przy pierwszym uruchomieniu lub czystej bazie.
                
                // Sprawdź, czy w bazie są już laptopy z tymi kodami
                if (!await DbContext.Laptopy.AnyAsync(l => l.KodKreskowy == "5907512566114"))
                {
                    DbContext.Laptopy.AddRange(
                        new Laptop { Marka = "Microsoft", Model = "Surface Pro 9", SystemOperacyjny = "Windows 11", RozmiarEkranu = 13.0, Ilosc = 4, KodKreskowy = "5907512566114" },
                        new Laptop { Marka = "Acer", Model = "Swift 5", SystemOperacyjny = "Windows 11", RozmiarEkranu = 14.0, Ilosc = 8, KodKreskowy = "5907512566121" },
                        new Laptop { Marka = "Dell", Model = "Inspiron 15", SystemOperacyjny = "Windows 11 Home", RozmiarEkranu = 15.6, Ilosc = 10, KodKreskowy = "5907512566138" },
                        new Laptop { Marka = "MSI", Model = "Creator Z16", SystemOperacyjny = "Windows 11 Pro", RozmiarEkranu = 16.0, Ilosc = 3, KodKreskowy = "5907512566145" },
                        new Laptop { Marka = "Razer", Model = "Blade 14", SystemOperacyjny = "Windows 11", RozmiarEkranu = 14.0, Ilosc = 5, KodKreskowy = "5907512566152" }
                    );
                    await DbContext.SaveChangesAsync();
                }
                return true; // Inicjalizacja pomyślna
            }
            catch (Exception ex)
            {
                // W przypadku błędu, zapisz go do pliku i pokaż użytkownikowi komunikat
                File.WriteAllText("db_error.log", $"Błąd inicjalizacji bazy danych: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                MessageBox.Show($"Wystąpił błąd podczas inicjalizacji bazy danych: {ex.Message}\n\nSzczegóły zapisano w pliku db_error.log\n\nAplikacja zostanie zamknięta.", "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);
                return false; // Inicjalizacja nieudana
            }
        }

        // Metoda wywoływana przy zamykaniu aplikacji
        protected override void OnExit(ExitEventArgs e)
        {
            // Zwolnij zasoby kontekstu bazy danych, aby uniknąć wycieków
            DbContext?.Dispose();
            base.OnExit(e);
        }
    }
}