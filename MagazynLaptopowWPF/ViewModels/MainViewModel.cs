using MagazynLaptopowWPF.Models;
using MagazynLaptopowWPF.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32;
using System.Windows;
using MagazynLaptopowWPF.Views;
using System.IO;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace MagazynLaptopowWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ILaptopRepository _laptopRepository;
        private readonly AppDbContext _dbContext;

        private ObservableCollection<Laptop> _laptopy;
        public ObservableCollection<Laptop> Laptopy
        {
            get => _laptopy;
            set => SetProperty(ref _laptopy, value);
        }

        // Używamy ICollectionView do sortowania i filtrowania w UI
        private ICollectionView _laptopyView;
        public ICollectionView LaptopyView
        {
            get => _laptopyView;
            set => SetProperty(ref _laptopyView, value);
        }

        private Laptop? _selectedLaptop;
        public Laptop? SelectedLaptop
        {
            get => _selectedLaptop;
            set
            {
                if (SetProperty(ref _selectedLaptop, value))
                {
                    // Powiadom komendy o zmianie możliwości wykonania
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        // Dodane dla wsparcia kodu kreskowego
        private string _filterKodKreskowy = string.Empty;
        public string FilterKodKreskowy
        {
            get => _filterKodKreskowy;
            set
            {
                if (SetProperty(ref _filterKodKreskowy, value))
                {
                    ApplyFilter(); // Filtruj przy każdej zmianie tekstu
                }
            }
        }

        private string _filterMarka = string.Empty;
        public string FilterMarka
        {
            get => _filterMarka;
            set
            {
                if (SetProperty(ref _filterMarka, value))
                {
                    ApplyFilter(); // Filtruj przy każdej zmianie tekstu
                }
            }
        }

        private string _filterModel = string.Empty;
        public string FilterModel
        {
            get => _filterModel;
            set
            {
                if (SetProperty(ref _filterModel, value))
                {
                    ApplyFilter(); // Filtruj przy każdej zmianie tekstu
                }
            }
        }

        private string _statusMessage = "Gotowy";
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        // Właściwości dla trybu szybkiego skanowania
        private bool _isQuickModeActive;
        public bool IsQuickModeActive
        {
            get => _isQuickModeActive;
            set
            {
                if (SetProperty(ref _isQuickModeActive, value))
                {
                    OnPropertyChanged(nameof(QuickModeBackground));
                    StatusMessage = value
                        ? "Tryb szybkiego dodawania aktywny. Zeskanuj kod kreskowy, aby zwiększyć ilość."
                        : "Tryb szybkiego dodawania wyłączony.";
                }
            }
        }

        public Brush QuickModeBackground
        {
            get => IsQuickModeActive ? Brushes.Green : (Brush)Application.Current.Resources["MahApps.Brushes.Gray8"];
        }

        private Laptop? _quickModeLaptop;

        // --- Komendy (Commands) ---
        public ICommand LoadDataCommand { get; }
        public ICommand AddLaptopCommand { get; }
        public ICommand EditLaptopCommand { get; }
        public ICommand DeleteLaptopCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand ToggleQuickModeCommand { get; }
        public ICommand SearchBarcodeCommand { get; }

        // Konstruktor
        public MainViewModel()
        {
            _laptopy = new ObservableCollection<Laptop>();
            _laptopyView = CollectionViewSource.GetDefaultView(_laptopy);

            // Korzystaj z DbContext zainicjowanego w App.xaml.cs zamiast tworzyć nowy
            _dbContext = App.DbContext ?? new AppDbContext();
            _laptopRepository = new LaptopRepository(_dbContext);

            // Definicje komend
            LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync(), _ => !IsBusy);
            AddLaptopCommand = new RelayCommand(_ => AddLaptop(), _ => !IsBusy);
            EditLaptopCommand = new RelayCommand(_ => EditLaptop(), _ => CanEditOrDeleteLaptop() && !IsBusy);
            DeleteLaptopCommand = new RelayCommand(async _ => await DeleteLaptopAsync(), _ => CanEditOrDeleteLaptop() && !IsBusy);
            ExportCommand = new RelayCommand(async _ => await ExportDataAsync(), _ => Laptopy.Count > 0 && !IsBusy);
            ImportCommand = new RelayCommand(async _ => await ImportDataAsync(), _ => !IsBusy);
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => !string.IsNullOrEmpty(FilterKodKreskowy) || !string.IsNullOrEmpty(FilterMarka) || !string.IsNullOrEmpty(FilterModel));
            ToggleQuickModeCommand = new RelayCommand(_ => ToggleQuickMode(), _ => !IsBusy);
            SearchBarcodeCommand = new RelayCommand(_ => SearchByBarcode(), _ => !string.IsNullOrEmpty(FilterKodKreskowy) && !IsBusy);

            // Konfiguracja sortowania
            _laptopyView.SortDescriptions.Add(new SortDescription("Marka", ListSortDirection.Ascending));
            _laptopyView.SortDescriptions.Add(new SortDescription("Model", ListSortDirection.Ascending));

            // Konfiguracja filtrowania
            _laptopyView.Filter = FilterLogic;

            // Ładuj dane w osobnym wątku, aby uniknąć blokowania UI po inicjalizacji
            Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await LoadDataAsync();
                }
                catch (Exception ex)
                {
                    // Logowanie wyjątku, ale nie pokazuj MessageBox w konstruktorze
                    Console.WriteLine($"Błąd ładowania danych: {ex.Message}");
                    StatusMessage = "Nie udało się załadować danych. Kliknij 'Odśwież', aby spróbować ponownie.";
                }
            });
        }

        // --- Metody ---
        public async Task LoadDataAsync()
        {
            StatusMessage = "Ładowanie danych...";
            IsBusy = true;
            try
            {
                var data = await _laptopRepository.GetAllLaptopsAsync();
                Laptopy.Clear(); // Wyczyść starą kolekcję
                foreach (var laptop in data)
                {
                    Laptopy.Add(laptop);
                }
                StatusMessage = $"Załadowano {Laptopy.Count} laptopów.";
                // Odśwież widok, aby zastosować filtrowanie
                LaptopyView.Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd ładowania danych: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas ładowania danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void AddLaptop()
        {
            var newLaptop = new Laptop
            {
                Ilosc = 1 // Domyślna wartość
            };

            var addEditVM = new AddEditLaptopViewModel(newLaptop);
            var dialog = new AddEditLaptopWindow(addEditVM);

            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                _ = AddLaptopInternalAsync(addEditVM.Laptop);
            }
        }

        private async Task AddLaptopInternalAsync(Laptop laptopToAdd)
        {
            IsBusy = true;
            StatusMessage = "Dodawanie laptopa...";
            try
            {
                // Sprawdź, czy kod kreskowy już istnieje
                if (!string.IsNullOrWhiteSpace(laptopToAdd.KodKreskowy) &&
                    await _laptopRepository.BarcodeExistsAsync(laptopToAdd.KodKreskowy))
                {
                    var result = MessageBox.Show(
                        $"Laptop z kodem kreskowym {laptopToAdd.KodKreskowy} już istnieje w bazie. Czy chcesz zwiększyć ilość istniejącego laptopa?",
                        "Duplikat kodu kreskowego",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _laptopRepository.IncrementLaptopQuantityAsync(laptopToAdd.KodKreskowy, laptopToAdd.Ilosc);
                        await LoadDataAsync(); // Odśwież dane
                        StatusMessage = $"Zwiększono ilość laptopa z kodem {laptopToAdd.KodKreskowy}";
                        return;
                    }
                    else
                    {
                        // Użytkownik nie chce zwiększać ilości - wyczyść kod kreskowy
                        laptopToAdd.KodKreskowy = null;
                    }
                }

                await _laptopRepository.AddLaptopAsync(laptopToAdd);
                Laptopy.Add(laptopToAdd);
                StatusMessage = $"Dodano: {laptopToAdd.Marka} {laptopToAdd.Model}";

                // Odśwież widok, aby zastosować sortowanie
                LaptopyView.Refresh();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd dodawania laptopa: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas dodawania laptopa:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void EditLaptop()
        {
            if (SelectedLaptop == null) return;

            // Tworzymy kopię, aby zmiany w oknie edycji nie wpływały od razu na DataGrid
            var laptopToEditCopy = new Laptop
            {
                Id = SelectedLaptop.Id,
                Marka = SelectedLaptop.Marka,
                Model = SelectedLaptop.Model,
                SystemOperacyjny = SelectedLaptop.SystemOperacyjny,
                RozmiarEkranu = SelectedLaptop.RozmiarEkranu,
                Ilosc = SelectedLaptop.Ilosc,
                KodKreskowy = SelectedLaptop.KodKreskowy
            };

            var addEditVM = new AddEditLaptopViewModel(laptopToEditCopy);
            var dialog = new AddEditLaptopWindow(addEditVM);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                _ = EditLaptopInternalAsync(addEditVM.Laptop);
            }
        }

        // Naprawiona metoda edycji laptopa - rozwiązanie problemu śledzenia encji
        private async Task EditLaptopInternalAsync(Laptop editedLaptop)
        {
            IsBusy = true;
            StatusMessage = "Aktualizowanie laptopa...";
            try
            {
                // Sprawdź, czy kod kreskowy jest unikalny (ale tylko jeśli się zmienił)
                var originalLaptop = await _laptopRepository.GetLaptopByIdAsync(editedLaptop.Id);
                if (originalLaptop != null &&
                    !string.IsNullOrWhiteSpace(editedLaptop.KodKreskowy) &&
                    editedLaptop.KodKreskowy != originalLaptop.KodKreskowy)
                {
                    var existingWithBarcode = await _laptopRepository.GetLaptopByBarcodeAsync(editedLaptop.KodKreskowy);
                    if (existingWithBarcode != null && existingWithBarcode.Id != editedLaptop.Id)
                    {
                        MessageBox.Show(
                            $"Laptop z kodem kreskowym {editedLaptop.KodKreskowy} już istnieje w bazie. Każdy kod kreskowy musi być unikalny.",
                            "Duplikat kodu kreskowego",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        // Przywróć oryginalny kod kreskowy
                        editedLaptop.KodKreskowy = originalLaptop.KodKreskowy;
                    }
                }

                // Aktualizuj właściwości oryginalnego obiektu
                originalLaptop.Marka = editedLaptop.Marka;
                originalLaptop.Model = editedLaptop.Model;
                originalLaptop.SystemOperacyjny = editedLaptop.SystemOperacyjny;
                originalLaptop.RozmiarEkranu = editedLaptop.RozmiarEkranu;
                originalLaptop.Ilosc = editedLaptop.Ilosc;
                originalLaptop.KodKreskowy = editedLaptop.KodKreskowy;

                // Zapisz zmiany
                await _dbContext.SaveChangesAsync();

                // Zaktualizuj wersję w kolekcji UI
                var uiLaptop = Laptopy.FirstOrDefault(l => l.Id == editedLaptop.Id);
                if (uiLaptop != null)
                {
                    uiLaptop.Marka = editedLaptop.Marka;
                    uiLaptop.Model = editedLaptop.Model;
                    uiLaptop.SystemOperacyjny = editedLaptop.SystemOperacyjny;
                    uiLaptop.RozmiarEkranu = editedLaptop.RozmiarEkranu;
                    uiLaptop.Ilosc = editedLaptop.Ilosc;
                    uiLaptop.KodKreskowy = editedLaptop.KodKreskowy;
                }

                // Odśwież widok
                LaptopyView.Refresh();
                StatusMessage = $"Zaktualizowano: {editedLaptop.Marka} {editedLaptop.Model}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd aktualizacji laptopa: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas aktualizacji laptopa:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteLaptopAsync()
        {
            if (SelectedLaptop == null) return;

            var result = MessageBox.Show($"Czy na pewno chcesz usunąć laptopa: {SelectedLaptop.Marka} {SelectedLaptop.Model}?",
                                         "Potwierdzenie usunięcia",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Usuwanie laptopa...";
                try
                {
                    await _laptopRepository.DeleteLaptopAsync(SelectedLaptop.Id);
                    Laptopy.Remove(SelectedLaptop); // Usuń z widocznej kolekcji
                    SelectedLaptop = null; // Odznacz element
                    StatusMessage = "Laptop usunięty.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Błąd usuwania laptopa: {ex.Message}";
                    MessageBox.Show($"Wystąpił błąd podczas usuwania laptopa:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private bool CanEditOrDeleteLaptop()
        {
            return SelectedLaptop != null;
        }

        // --- Filtrowanie ---
        private void ApplyFilter()
        {
            LaptopyView.Refresh();
            // Aktualizuj status o liczbie widocznych elementów
            int visibleCount = Laptopy.Count(item => FilterLogic(item));
            StatusMessage = $"Widocznych: {visibleCount} z {Laptopy.Count} laptopów.";
        }

        private void ClearFilters()
        {
            FilterKodKreskowy = string.Empty;
            FilterMarka = string.Empty;
            FilterModel = string.Empty;
            StatusMessage = "Filtry wyczyszczone.";
        }

        // Logika filtrowania używana przez ICollectionView
        private bool FilterLogic(object item)
        {
            if (item is Laptop laptop)
            {
                bool barcodeMatch = string.IsNullOrWhiteSpace(FilterKodKreskowy) ||
                                    (laptop.KodKreskowy != null && laptop.KodKreskowy.Contains(FilterKodKreskowy));

                bool markaMatch = string.IsNullOrWhiteSpace(FilterMarka) ||
                                 (laptop.Marka != null && laptop.Marka.Contains(FilterMarka, StringComparison.OrdinalIgnoreCase));

                bool modelMatch = string.IsNullOrWhiteSpace(FilterModel) ||
                                 (laptop.Model != null && laptop.Model.Contains(FilterModel, StringComparison.OrdinalIgnoreCase));

                return barcodeMatch && markaMatch && modelMatch;
            }
            return false;
        }

        // --- Funkcje dla kodu kreskowego ---

        // Przełączanie trybu szybkiego skanowania
        private void ToggleQuickMode()
        {
            IsQuickModeActive = !IsQuickModeActive;
            _quickModeLaptop = null;

            if (IsQuickModeActive)
            {
                // Jeśli włączamy tryb szybki, wyczyść wszystkie filtry
                ClearFilters();
            }
        }

        // Wyszukiwanie laptopa po kodzie kreskowym
        public void SearchByBarcode()
        {
            if (string.IsNullOrWhiteSpace(FilterKodKreskowy))
                return;

            // Szukaj dokładnego dopasowania
            var foundLaptop = Laptopy.FirstOrDefault(l => l.KodKreskowy == FilterKodKreskowy);
            if (foundLaptop != null)
            {
                // Jeśli znaleziono, zaznacz na liście
                SelectedLaptop = foundLaptop;

                // Przewiń do wybranego laptopa (to będzie obsługiwane przez UI)
                StatusMessage = $"Znaleziono: {foundLaptop.Marka} {foundLaptop.Model}";
            }
            else
            {
                StatusMessage = $"Nie znaleziono laptopa o kodzie: {FilterKodKreskowy}";

                // Zapytaj, czy dodać nowy laptop z tym kodem
                var result = MessageBox.Show(
                    $"Nie znaleziono laptopa o kodzie kreskowym: {FilterKodKreskowy}\n\nCzy chcesz dodać nowy laptop z tym kodem?",
                    "Laptop nie znaleziony",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var newLaptop = new Laptop
                    {
                        KodKreskowy = FilterKodKreskowy,
                        Ilosc = 1
                    };

                    var addEditVM = new AddEditLaptopViewModel(newLaptop);
                    var dialog = new AddEditLaptopWindow(addEditVM);
                    dialog.Owner = Application.Current.MainWindow;

                    if (dialog.ShowDialog() == true)
                    {
                        _ = AddLaptopInternalAsync(addEditVM.Laptop);
                    }
                }
            }
        }

        // Obsługa kodu kreskowego w trybie szybkim
        public async void ProcessBarcodeInQuickMode()
        {
            if (!IsQuickModeActive || string.IsNullOrWhiteSpace(FilterKodKreskowy))
                return;

            try
            {
                IsBusy = true;

                // Szukaj laptopa o podanym kodzie kreskowym
                var foundLaptop = await _laptopRepository.GetLaptopByBarcodeAsync(FilterKodKreskowy);

                // Jeśli znaleziono laptop z tym kodem kreskowym
                if (foundLaptop != null)
                {
                    // Zwiększ ilość
                    foundLaptop.Ilosc++;
                    await _laptopRepository.UpdateLaptopAsync(foundLaptop);

                    // Aktualizuj widok
                    var uiLaptop = Laptopy.FirstOrDefault(l => l.Id == foundLaptop.Id);
                    if (uiLaptop != null)
                    {
                        uiLaptop.Ilosc = foundLaptop.Ilosc;
                        SelectedLaptop = uiLaptop; // Zaznacz zaktualizowany laptop
                    }

                    StatusMessage = $"Zwiększono ilość: {foundLaptop.Marka} {foundLaptop.Model} - {foundLaptop.Ilosc} szt.";
                    LaptopyView.Refresh();
                }
                else
                {
                    // Jeśli nie znaleziono, pytamy czy dodać nowy
                    var result = MessageBox.Show(
                        $"Nie znaleziono laptopa o kodzie {FilterKodKreskowy}. Czy chcesz dodać nowy laptop?",
                        "Laptop nie znaleziony",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        var newLaptop = new Laptop
                        {
                            KodKreskowy = FilterKodKreskowy,
                            Ilosc = 1
                        };

                        var addEditVM = new AddEditLaptopViewModel(newLaptop);
                        var dialog = new AddEditLaptopWindow(addEditVM);
                        dialog.Owner = Application.Current.MainWindow;

                        if (dialog.ShowDialog() == true)
                        {
                            await AddLaptopInternalAsync(addEditVM.Laptop);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd przetwarzania kodu kreskowego: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas przetwarzania kodu kreskowego:\n{ex.Message}",
                               "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                FilterKodKreskowy = string.Empty; // Wyczyść pole po przetworzeniu
            }
        }

        // --- Import / Eksport ---
        private async Task ExportDataAsync()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Pliki CSV (*.csv)|*.csv|Wszystkie pliki (*.*)|*.*",
                Title = "Eksportuj dane do CSV",
                FileName = $"MagazynLaptopow_{DateTime.Now:yyyyMMdd}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                StatusMessage = "Eksportowanie danych...";
                try
                {
                    // Zrobić widok filtrowany
                    var dataToExport = LaptopyView.Cast<Laptop>().ToList();

                    using (var writer = new StreamWriter(saveFileDialog.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Nagłówek
                        await writer.WriteLineAsync("Id;KodKreskowy;Marka;Model;SystemOperacyjny;RozmiarEkranu;Ilosc");

                        // Dane
                        foreach (var laptop in dataToExport)
                        {
                            var line = string.Join(";",
                                laptop.Id,
                                EscapeCsvField(laptop.KodKreskowy ?? ""),
                                EscapeCsvField(laptop.Marka),
                                EscapeCsvField(laptop.Model),
                                EscapeCsvField(laptop.SystemOperacyjny ?? ""),
                                laptop.RozmiarEkranu?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "",
                                laptop.Ilosc
                            );
                            await writer.WriteLineAsync(line);
                        }
                    }

                    StatusMessage = $"Dane wyeksportowane do: {Path.GetFileName(saveFileDialog.FileName)}";
                    MessageBox.Show($"Dane zostały wyeksportowane do pliku:\n{saveFileDialog.FileName}",
                                   "Eksport zakończony", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Błąd eksportu: {ex.Message}";
                    MessageBox.Show($"Wystąpił błąd podczas eksportu danych:\n{ex.Message}",
                                   "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return string.Empty;

            if (field.Contains(';') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        private async Task ImportDataAsync()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Pliki CSV (*.csv)|*.csv|Wszystkie pliki (*.*)|*.*",
                Title = "Importuj dane z CSV"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsBusy = true;
                StatusMessage = "Importowanie danych...";
                try
                {
                    var importedLaptops = new List<Laptop>();
                    using (var reader = new StreamReader(openFileDialog.FileName, System.Text.Encoding.UTF8))
                    {
                        string? headerLine = await reader.ReadLineAsync();
                        if (headerLine == null) throw new Exception("Plik CSV jest pusty lub nieprawidłowy.");

                        // Sprawdź czy nagłówek zawiera KodKreskowy
                        bool hasBarcode = headerLine.Contains("KodKreskowy");

                        string? line;
                        int lineNumber = 1;
                        int successCount = 0;
                        int errorCount = 0;

                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineNumber++;
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            try
                            {
                                var fields = ParseCsvLine(line);

                                // Różna liczba kolumn w zależności od obecności kodu kreskowego
                                int startIdx = hasBarcode ? 1 : 0;

                                if (fields.Length >= (hasBarcode ? 6 : 5))
                                {
                                    var laptop = new Laptop
                                    {
                                        KodKreskowy = hasBarcode ? fields[startIdx].Trim() : null,
                                        Marka = fields[startIdx + 1].Trim(),
                                        Model = fields[startIdx + 2].Trim(),
                                        SystemOperacyjny = string.IsNullOrWhiteSpace(fields[startIdx + 3]) ? null : fields[startIdx + 3].Trim(),
                                        RozmiarEkranu = double.TryParse(fields[startIdx + 4].Trim(), System.Globalization.NumberStyles.Any,
                                                        System.Globalization.CultureInfo.InvariantCulture, out double size) ? size : (double?)null,
                                        Ilosc = int.TryParse(fields[startIdx + 5].Trim(), out int qty) ? qty : 0
                                    };

                                    if (!string.IsNullOrWhiteSpace(laptop.Marka) && !string.IsNullOrWhiteSpace(laptop.Model))
                                    {
                                        importedLaptops.Add(laptop);
                                        successCount++;
                                    }
                                    else
                                    {
                                        errorCount++;
                                    }
                                }
                                else
                                {
                                    errorCount++;
                                }
                            }
                            catch
                            {
                                errorCount++;
                            }
                        }

                        StatusMessage = $"Zaimportowano {successCount} laptopów. Błędnych wierszy: {errorCount}.";
                    }

                    if (importedLaptops.Any())
                    {
                        var confirmResult = MessageBox.Show($"Zaimportowano {importedLaptops.Count} laptopów. Czy chcesz dodać je do bazy danych?",
                                                          "Potwierdzenie importu", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (confirmResult == MessageBoxResult.Yes)
                        {
                            foreach (var laptop in importedLaptops)
                            {
                                // Sprawdź, czy kod kreskowy już istnieje
                                if (!string.IsNullOrWhiteSpace(laptop.KodKreskowy) &&
                                    await _laptopRepository.BarcodeExistsAsync(laptop.KodKreskowy))
                                {
                                    // Zwiększ ilość istniejącego laptopa
                                    await _laptopRepository.IncrementLaptopQuantityAsync(laptop.KodKreskowy, laptop.Ilosc);
                                }
                                else
                                {
                                    await _laptopRepository.AddLaptopAsync(laptop);
                                }
                            }

                            await LoadDataAsync(); // Odśwież widok
                            StatusMessage = $"Dodano/zaktualizowano {importedLaptops.Count} laptopów w bazie danych.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Błąd importu: {ex.Message}";
                    MessageBox.Show($"Wystąpił błąd podczas importu danych:\n{ex.Message}",
                                   "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        // Lepsza implementacja parsowania CSV uwzględniająca pola w cudzysłowach
        private string[] ParseCsvLine(string line)
        {
            List<string> fields = new List<string>();
            bool inQuotes = false;
            int fieldStart = 0;

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (line[i] == ';' && !inQuotes)
                {
                    fields.Add(line.Substring(fieldStart, i - fieldStart).Trim('"'));
                    fieldStart = i + 1;
                }
            }

            // Dodaj ostatnie pole
            fields.Add(line.Substring(fieldStart).Trim('"'));

            return fields.ToArray();
        }
    }

    // Prosta implementacja ICommand (RelayCommand)
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}