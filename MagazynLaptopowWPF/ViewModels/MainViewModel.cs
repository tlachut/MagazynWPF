using MagazynLaptopowWPF.Models;
using MagazynLaptopowWPF.Services; // Potrzebne repozytorium
using System.Collections.ObjectModel; // Dla ObservableCollection
using System.ComponentModel; // Dla ICollectionView
using System.Windows.Data; // Dla CollectionViewSource
using System.Windows.Input; // Dla ICommand
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Win32; // Dla dialogów plików
using System.Windows; // Dla MessageBox
using MagazynLaptopowWPF.Views; // Dla AddEditLaptopWindow
using MagazynLaptopowWPF.ViewModels; // Dla AddEditLaptopViewModel (jeśli jeszcze nie ma)

namespace MagazynLaptopowWPF.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ILaptopRepository _laptopRepository;
        // private readonly ICsvService _csvService; // Do wstrzyknięcia serwisu CSV
        // private readonly IDialogService _dialogService; // Do wstrzyknięcia serwisu dialogów

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
                    // Ważne: Powiadom komendy o zmianie możliwości wykonania
                    ((RelayCommand)EditLaptopCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteLaptopCommand).RaiseCanExecuteChanged();
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


        // --- Komendy (Commands) ---
        public ICommand LoadDataCommand { get; }
        public ICommand AddLaptopCommand { get; }
        public ICommand EditLaptopCommand { get; }
        public ICommand DeleteLaptopCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }


        // Konstruktor
        public MainViewModel(/*ILaptopRepository laptopRepository, ICsvService csvService, IDialogService dialogService*/)
        {
            // --- Wstrzykiwanie zależności (lepsze podejście) ---
            // _laptopRepository = laptopRepository;
            // _csvService = csvService;
            // _dialogService = dialogService;

            // --- Prostsze podejście bez DI (na początek) ---
            var dbContext = new AppDbContext();
            _laptopRepository = new LaptopRepository(dbContext);
            // _csvService = new CsvService(); // Musisz utworzyć tę klasę
            // _dialogService = new DialogService(); // Musisz utworzyć tę klasę

            _laptopy = new ObservableCollection<Laptop>();
            _laptopyView = CollectionViewSource.GetDefaultView(_laptopy); // Tworzymy widok kolekcji

            // Definicje komend (użyjemy prostej implementacji RelayCommand poniżej)
            LoadDataCommand = new RelayCommand(async _ => await LoadDataAsync());
            AddLaptopCommand = new RelayCommand(_ => AddLaptop());
            EditLaptopCommand = new RelayCommand(_ => EditLaptop(), _ => CanEditOrDeleteLaptop()); // CanExecute
            DeleteLaptopCommand = new RelayCommand(async _ => await DeleteLaptopAsync(), _ => CanEditOrDeleteLaptop()); // CanExecute
            ExportCommand = new RelayCommand(async _ => await ExportDataAsync());
            ImportCommand = new RelayCommand(async _ => await ImportDataAsync());


            // Konfiguracja sortowania (można dodać w XAML lub tutaj)
            _laptopyView.SortDescriptions.Add(new SortDescription("Marka", ListSortDirection.Ascending));
            _laptopyView.SortDescriptions.Add(new SortDescription("Model", ListSortDirection.Ascending));

            // Konfiguracja filtrowania
            _laptopyView.Filter = FilterLogic;


            // Załaduj dane przy starcie
            _ = LoadDataAsync(); // Uruchom asynchronicznie bez czekania
        }

        // --- Metody ---

        private async Task LoadDataAsync()
        {
            StatusMessage = "Ładowanie danych...";
            try
            {
                var data = await _laptopRepository.GetAllLaptopsAsync();
                Laptopy.Clear(); // Wyczyść starą kolekcję
                foreach (var laptop in data)
                {
                    Laptopy.Add(laptop);
                }
                StatusMessage = $"Załadowano {Laptopy.Count} laptopów.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd ładowania danych: {ex.Message}";
                // Tutaj obsługa błędów, np. MessageBox
                MessageBox.Show($"Wystąpił błąd podczas ładowania danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddLaptop()
        {
            var newLaptop = new Laptop(); // Pusty laptop do dodania
            var addEditVM = new AddEditLaptopViewModel(newLaptop); // ViewModel dla okna dialogowego
            var dialog = new AddEditLaptopWindow(addEditVM); // Tworzymy okno

            // Opcjonalnie: Ustaw właściciela, aby okno było modalne
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true) // Pokaż okno i sprawdź wynik
            {
                // Jeśli użytkownik kliknął "Zapisz" (zakładamy, że ViewModel ustawił IsSaved)
                _ = AddLaptopInternalAsync(addEditVM.Laptop); // Uruchom dodawanie asynchronicznie
            }
        }

        private async Task AddLaptopInternalAsync(Laptop laptopToAdd)
        {
            try
            {
                await _laptopRepository.AddLaptopAsync(laptopToAdd);
                Laptopy.Add(laptopToAdd); // Dodaj do widocznej kolekcji
                StatusMessage = $"Dodano: {laptopToAdd.Marka} {laptopToAdd.Model}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd dodawania laptopa: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas dodawania laptopa:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
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
                // Skopiuj inne pola jeśli istnieją
            };

            var addEditVM = new AddEditLaptopViewModel(laptopToEditCopy);
            var dialog = new AddEditLaptopWindow(addEditVM);
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                _ = EditLaptopInternalAsync(addEditVM.Laptop); // Uruchom edycję asynchronicznie
            }
        }

        private async Task EditLaptopInternalAsync(Laptop editedLaptop)
        {
            try
            {
                await _laptopRepository.UpdateLaptopAsync(editedLaptop);

                // Znajdź oryginalny obiekt w kolekcji i zaktualizuj jego właściwości
                var originalLaptop = Laptopy.FirstOrDefault(l => l.Id == editedLaptop.Id);
                if (originalLaptop != null)
                {
                    originalLaptop.Marka = editedLaptop.Marka;
                    originalLaptop.Model = editedLaptop.Model;
                    originalLaptop.SystemOperacyjny = editedLaptop.SystemOperacyjny;
                    originalLaptop.RozmiarEkranu = editedLaptop.RozmiarEkranu;
                    originalLaptop.Ilosc = editedLaptop.Ilosc;
                    // Zaktualizuj inne pola

                    // Ważne: Ręczne odświeżenie widoku, jeśli edycja była na kopii
                    // Lub jeśli Laptop nie implementuje INotifyPropertyChanged
                    LaptopyView.Refresh();
                }

                StatusMessage = $"Zaktualizowano: {editedLaptop.Marka} {editedLaptop.Model}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Błąd aktualizacji laptopa: {ex.Message}";
                MessageBox.Show($"Wystąpił błąd podczas aktualizacji laptopa:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private async Task DeleteLaptopAsync()
        {
            if (SelectedLaptop == null) return;

            // Potwierdzenie usunięcia
            var result = MessageBox.Show($"Czy na pewno chcesz usunąć laptopa: {SelectedLaptop.Marka} {SelectedLaptop.Model}?",
                                         "Potwierdzenie usunięcia",
                                         MessageBoxButton.YesNo,
                                         MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
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
            }
        }

        private bool CanEditOrDeleteLaptop()
        {
            return SelectedLaptop != null; // Można edytować/usuwać tylko jeśli coś jest zaznaczone
        }

        // --- Filtrowanie ---
        private void ApplyFilter()
        {
            // Odśwież widok, aby zastosować logikę filtrowania
            LaptopyView.Refresh();
            // Aktualizuj status o liczbie widocznych elementów
            int visibleCount = Laptopy.Count(item => FilterLogic(item)); // Ponownie zastosuj logikę, aby policzyć
            StatusMessage = $"Widocznych: {visibleCount} z {Laptopy.Count} laptopów.";

        }

        // Logika filtrowania używana przez ICollectionView
        private bool FilterLogic(object item)
        {
            if (item is Laptop laptop)
            {
                bool markaMatch = string.IsNullOrWhiteSpace(FilterMarka) ||
                                 (laptop.Marka != null && laptop.Marka.Contains(FilterMarka, StringComparison.OrdinalIgnoreCase)); // Ignoruj wielkość liter

                bool modelMatch = string.IsNullOrWhiteSpace(FilterModel) ||
                                 (laptop.Model != null && laptop.Model.Contains(FilterModel, StringComparison.OrdinalIgnoreCase));

                return markaMatch && modelMatch; // Laptop pasuje, jeśli pasuje do obu filtrów (lub filtry są puste)
            }
            return false; // Nieznany typ obiektu
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
                StatusMessage = "Eksportowanie danych...";
                try
                {
                    // Pobierz dane do eksportu (mogą być filtrowane lub wszystkie)
                    // W tym przykładzie eksportujemy WSZYSTKIE dane z bazy
                    // Jeśli chcesz eksportować tylko widoczne (przefiltrowane), użyj LaptopyView
                    var dataToExport = await _laptopRepository.GetAllLaptopsAsync();

                    // Użyj serwisu CSV do zapisu (wymaga implementacji CsvService)
                    // await _csvService.ExportLaptopsAsync(dataToExport, saveFileDialog.FileName);

                    // --- Prosta implementacja zapisu CSV bez CsvHelper ---
                    using (var writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        // Nagłówek
                        await writer.WriteLineAsync("Id;Marka;Model;SystemOperacyjny;RozmiarEkranu;Ilosc");
                        // Dane
                        foreach (var laptop in dataToExport)
                        {
                            // Użyj średnika jako separatora, obsłuż potencjalne nulle i formatowanie
                            var line = string.Join(";",
                                laptop.Id,
                                EscapeCsvField(laptop.Marka),
                                EscapeCsvField(laptop.Model),
                                EscapeCsvField(laptop.SystemOperacyjny ?? ""), // Zastąp null pustym stringiem
                                laptop.RozmiarEkranu?.ToString(System.Globalization.CultureInfo.InvariantCulture) ?? "", // Formatuj liczbę z kropką
                                laptop.Ilosc
                            );
                            await writer.WriteLineAsync(line);
                        }
                    }
                    // --- Koniec prostej implementacji ---

                    StatusMessage = $"Dane wyeksportowane do: {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Błąd eksportu: {ex.Message}";
                    MessageBox.Show($"Wystąpił błąd podczas eksportu danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Pomocnicza metoda do obsługi znaków specjalnych w CSV (np. średniki w tekście)
        private string EscapeCsvField(string field)
        {
            if (field.Contains(';') || field.Contains('"') || field.Contains('\n'))
            {
                // Otocz pole cudzysłowami i podwój istniejące cudzysłowy
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
                StatusMessage = "Importowanie danych...";
                try
                {
                    // Użyj serwisu CSV do odczytu (wymaga implementacji CsvService)
                    // var importedLaptops = await _csvService.ImportLaptopsAsync(openFileDialog.FileName);

                    // --- Prosta implementacja odczytu CSV bez CsvHelper ---
                    var importedLaptops = new List<Laptop>();
                    using (var reader = new System.IO.StreamReader(openFileDialog.FileName))
                    {
                        string? headerLine = await reader.ReadLineAsync(); // Odczytaj i zignoruj nagłówek (lub zweryfikuj)
                        if (headerLine == null) throw new Exception("Plik CSV jest pusty lub nieprawidłowy.");

                        string? line;
                        int lineNumber = 1; // Do śledzenia błędów
                        while ((line = await reader.ReadLineAsync()) != null)
                        {
                            lineNumber++;
                            var fields = line.Split(';'); // Rozdziel po średniku (prosty przypadek)
                                                          // UWAGA: To nie obsłuży poprawnie średników wewnątrz pól otoczonych cudzysłowami!
                                                          // Dla poprawnej obsługi potrzebny jest parser CSV jak CsvHelper.

                            if (fields.Length >= 5) // Oczekujemy co najmniej 5 pól (bez ID)
                            {
                                try
                                {
                                    var laptop = new Laptop
                                    {
                                        // ID zostanie nadane przez bazę danych przy dodawaniu
                                        Marka = fields[1].Trim(), // Usuń białe znaki
                                        Model = fields[2].Trim(),
                                        SystemOperacyjny = string.IsNullOrWhiteSpace(fields[3]) ? null : fields[3].Trim(),
                                        RozmiarEkranu = double.TryParse(fields[4].Trim(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double size) ? size : (double?)null,
                                        Ilosc = int.TryParse(fields[5].Trim(), out int qty) ? qty : 0
                                    };
                                    // Podstawowa walidacja
                                    if (!string.IsNullOrWhiteSpace(laptop.Marka) && !string.IsNullOrWhiteSpace(laptop.Model))
                                    {
                                        importedLaptops.Add(laptop);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Pominięto wiersz {lineNumber}: Brak marki lub modelu.");
                                        // Można logować błędy bardziej szczegółowo
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Błąd przetwarzania wiersza {lineNumber}: {ex.Message}");
                                    // Można logować błędy
                                }
                            }
                        }
                    }
                    // --- Koniec prostej implementacji ---


                    if (importedLaptops.Any())
                    {
                        // Opcjonalnie: Zapytaj użytkownika, czy dodać czy zastąpić istniejące dane
                        var confirmResult = MessageBox.Show($"Zaimportowano {importedLaptops.Count} laptopów. Czy chcesz dodać je do bazy danych?",
                                                            "Potwierdzenie importu", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (confirmResult == MessageBoxResult.Yes)
                        {
                            int addedCount = 0;
                            foreach (var laptop in importedLaptops)
                            {
                                // Można dodać logikę sprawdzania duplikatów przed dodaniem
                                await _laptopRepository.AddLaptopAsync(laptop);
                                addedCount++;
                            }
                            StatusMessage = $"Zaimportowano i dodano {addedCount} laptopów.";
                            await LoadDataAsync(); // Odśwież widok
                        }
                        else
                        {
                            StatusMessage = "Import anulowany.";
                        }
                    }
                    else
                    {
                        StatusMessage = "Nie znaleziono prawidłowych danych do importu w pliku.";
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Błąd importu: {ex.Message}";
                    MessageBox.Show($"Wystąpił błąd podczas importu danych:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

    }

    // --- Prosta implementacja ICommand (RelayCommand) ---
    // Można użyć gotowych bibliotek MVVM (np. CommunityToolkit.Mvvm) dla bardziej rozbudowanych wersji
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

        // Metoda do ręcznego wywołania sprawdzenia CanExecute
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}