using System.Windows;
using System.Windows.Input;
using MagazynLaptopowWPF.ViewModels; // Upewnij się, że ten using jest

namespace MagazynLaptopowWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        // Usuwamy pole ViewModel - będziemy pobierać DataContext bezpośrednio

        public MainWindow()
        {
            InitializeComponent();

            // NIE ustawiamy DataContext tutaj - zostanie ustawiony w App.xaml.cs
            // DataContext = new MainViewModel(); // USUNIĘTE

            // Obsługa klawisza F5 do odświeżania danych
            KeyDown += (s, e) =>
            {
                var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
                if (e.Key == Key.F5 && viewModel != null)
                {
                    viewModel.LoadDataCommand.Execute(null);
                }
            };

            // Obsługa klawisza Delete
            KeyDown += (s, e) =>
            {
                var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
                if (e.Key == Key.Delete && viewModel != null && viewModel.SelectedLaptop != null)
                {
                    viewModel.DeleteLaptopCommand.Execute(null);
                }
            };

            // Ustawienie fokusu na pole kodu kreskowego po załadowaniu okna
            Loaded += (s, e) =>
            {
                txtFilterKodKreskowy.Focus();
            };

            // Obsługa globalnej reakcji na klawisze
            PreviewKeyDown += (s, e) =>
            {
                var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
                if (viewModel != null && viewModel.IsQuickModeActive)
                {
                    // W trybie szybkim, przechwytuj każde naciśnięcie klawisza,
                    // jeśli fokus nie jest już w polu skanowania
                    if (!(Keyboard.FocusedElement is System.Windows.Controls.TextBox textBox &&
                         textBox.Name == "txtFilterKodKreskowy"))
                    {
                        txtFilterKodKreskowy.Focus();

                        // Jeśli to są alfanumeryczne znaki, pozwól im przejść do TextBoxa
                        if (e.Key >= Key.A && e.Key <= Key.Z ||
                            e.Key >= Key.D0 && e.Key <= Key.D9 ||
                            e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
                        {
                            // Nie oznaczaj jako obsłużone, aby pozwolić znakowi dotrzeć do TextBoxa
                            return;
                        }
                    }
                }
            };
        }

        // Metoda pomocnicza do pobierania ViewModel z DataContext
        private MainViewModel? GetViewModel() => DataContext as MainViewModel;

        private void FilterKodKreskowy_KeyDown(object sender, KeyEventArgs e)
        {
            var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
            // Obsługa klawisza Enter w polu filtrowania kodu kreskowego
            if (e.Key == Key.Enter && viewModel != null)
            {
                // W trybie szybkim, Enter oznacza zatwierdzenie kodu i zwiększenie liczby sztuk
                if (viewModel.IsQuickModeActive)
                {
                    viewModel.ProcessBarcodeInQuickMode();
                    e.Handled = true;
                }
                // W normalnym trybie, Enter powoduje wyszukiwanie
                else
                {
                    viewModel.SearchByBarcode();
                    e.Handled = true;
                }
            }
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
            e.CanExecute = viewModel != null && viewModel.SelectedLaptop != null;
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var viewModel = GetViewModel(); // Pobierz ViewModel dynamicznie
            if (viewModel != null)
            {
                viewModel.DeleteLaptopCommand.Execute(null);
            }
        }
    }
}