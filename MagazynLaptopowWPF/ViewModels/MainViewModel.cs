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

        // --- Komendy (Commands) ---
        public ICommand LoadDataCommand { get; }
        public ICommand AddLaptopCommand { get; }
        public ICommand EditLaptopCommand { get; }
        public ICommand DeleteLaptopCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand ClearFiltersCommand { get; }

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
            ClearFiltersCommand = new RelayCommand(_ => ClearFilters(), _ => !string.IsNullOrEmpty(FilterMarka) || !string.IsNullOrEmpty(FilterModel));

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
                await _laptopRepository.AddLaptopAsync(laptopToAdd);
                Laptopy.Add(laptopToAdd); // Poprawiona nazwa z Laptops na Laptopy
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
                Ilosc = SelectedLaptop.Ilosc
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
                // Pobierz oryginalny obiekt z bazy danych
                var originalLaptop = await _laptopRepository.GetLaptopByIdAsync(editedLaptop.Id);

                if (originalLaptop != null)
                {
                    // Aktualizuj właściwości oryginalnego obiektu
                    originalLaptop.Marka = editedLaptop.Marka;
                    originalLaptop.Model = editedLaptop.Model;
                    originalLaptop.SystemOperacyjny = editedLaptop.SystemOperacyjny;
                    originalLaptop.RozmiarEkranu = editedLaptop.RozmiarEkranu;
                    originalLaptop.Ilosc = editedLaptop.Ilosc;

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
                    }

                    // Odśwież widok
                    LaptopyView.Refresh();
                    StatusMessage = $"Zaktualizowano: {editedLaptop.Marka} {editedLaptop.Model}";
                }
                else
                {
                    throw new Exception("Nie znaleziono laptopa o podanym Id.");
                }
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
            FilterMarka = string.Empty;
            FilterModel = string.Empty;
            StatusMessage = "Filtry wyczyszczone.";
        }

        // Logika filtrowania używana przez ICollectionView
        private bool FilterLogic(object item)
        {
            if (item is Laptop laptop)
            {
                bool markaMatch = string.IsNullOrWhiteSpace(FilterMarka) ||
                                 (laptop.Marka != null && laptop.Marka.Contains(FilterMarka, StringComparison.OrdinalIgnoreCase));

                bool modelMatch = string.IsNullOrWhiteSpace(FilterModel) ||
                                 (laptop.Model != null && laptop.Model.Contains(FilterModel, StringComparison.OrdinalIgnoreCase));

                return markaMatch && modelMatch;
            }
            return false;
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
                        await writer.WriteLineAsync("Id;Marka;Model;SystemOperacyjny;RozmiarEkranu;Ilosc");

                        // Dane
                        foreach (var laptop in dataToExport)
                        {
                            var line = string.Join(";",
                                laptop.Id,
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

                                if (fields.Length >= 5)
                                {
                                    var laptop = new Laptop
                                    {
                                        Marka = fields[1].Trim(),
                                        Model = fields[2].Trim(),
                                        SystemOperacyjny = string.IsNullOrWhiteSpace(fields[3]) ? null : fields[3].Trim(),
                                        RozmiarEkranu = double.TryParse(fields[4].Trim(), System.Globalization.NumberStyles.Any,
                                                        System.Globalization.CultureInfo.InvariantCulture, out double size) ? size : (double?)null,
                                        Ilosc = int.TryParse(fields[5].Trim(), out int qty) ? qty : 0
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
                                await _laptopRepository.AddLaptopAsync(laptop);
                            }

                            await LoadDataAsync(); // Odśwież widok
                            StatusMessage = $"Dodano {importedLaptops.Count} laptopów do bazy danych.";
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