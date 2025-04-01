using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using MagazynLaptopowWPF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MagazynLaptopowWPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    // Dodaj właściwość DbContext na poziomie aplikacji, aby zapewnić jeden kontekst bazy danych
    public static AppDbContext? DbContext { get; private set; }

    // Dodaj właściwość do przechowywania ścieżki bazy danych, aby była dostępna dla całej aplikacji
    public static string DbPath { get; private set; } = Path.Combine(Directory.GetCurrentDirectory(), "MagazynDb.sqlite");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Sprawdź, czy plik bazy danych istnieje i ma odpowiednie uprawnienia
        try
        {
            // Upewnij się, że katalog docelowy istnieje
            string? directory = Path.GetDirectoryName(DbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Usuń istniejący plik bazy danych
            if (File.Exists(DbPath))
            {
                try
                {
                    File.Delete(DbPath);
                }
                catch (Exception fileEx)
                {
                    MessageBox.Show($"Nie można usunąć pliku bazy danych: {fileEx.Message}",
                                    "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Utwórz kontekst bazy danych z odpowiednią ścieżką i konfiguracją
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder
                .UseSqlite($"Data Source={DbPath}")
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning);
                })
                .EnableSensitiveDataLogging(); // Włącz logowanie szczegółów dla lepszej diagnostyki

            DbContext = new AppDbContext(optionsBuilder.Options);

            // Utwórz bazę danych i zastosuj migracje
            await DbContext.Database.EnsureCreatedAsync();

            // Dodaj przykładowe dane, jeśli tabela jest pusta
            if (!await DbContext.Laptopy.AnyAsync())
            {
                DbContext.Laptopy.AddRange(
                    new Laptop { Marka = "Dell", Model = "XPS 13", SystemOperacyjny = "Windows 11", RozmiarEkranu = 13.4, Ilosc = 5 },
                    new Laptop { Marka = "Apple", Model = "MacBook Air M2", SystemOperacyjny = "macOS Sonoma", RozmiarEkranu = 13.6, Ilosc = 3 },
                    new Laptop { Marka = "Lenovo", Model = "ThinkPad X1 Carbon", SystemOperacyjny = "Windows 11 Pro", RozmiarEkranu = 14.0, Ilosc = 7 },
                    new Laptop { Marka = "HP", Model = "Spectre x360", SystemOperacyjny = "Windows 11", RozmiarEkranu = 15.6, Ilosc = 2 },
                    new Laptop { Marka = "Asus", Model = "ZenBook 14", SystemOperacyjny = "Windows 12", RozmiarEkranu = 13.6, Ilosc = 12 }
                );
                await DbContext.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Zapisz szczegółowy komunikat błędu do pliku
            File.WriteAllText("db_error.log",
                $"Błąd inicjalizacji bazy danych: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");

            MessageBox.Show($"Wystąpił błąd podczas inicjalizacji bazy danych: {ex.Message}\n\n" +
                           $"Szczegóły zapisano w pliku db_error.log\n\n" +
                           $"Aplikacja zostanie zamknięta.",
                           "Błąd krytyczny", MessageBoxButton.OK, MessageBoxImage.Error);

            // Zamknij aplikację w przypadku krytycznego błędu z bazą danych
            Current.Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Zamknij kontekst bazy danych przy zamykaniu aplikacji
        DbContext?.Dispose();
        base.OnExit(e);
    }
}