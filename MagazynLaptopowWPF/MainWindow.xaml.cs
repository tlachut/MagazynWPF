// "MainWindow.xaml.cs"

using System.Windows;
using System.Windows.Input;
using MagazynLaptopowWPF.ViewModels;

namespace MagazynLaptopowWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainViewModel? ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();

            // Ustaw DataContext w kodzie zamiast w XAML
            DataContext = new MainViewModel();

            // Obsługa klawisza F5 do odświeżania danych
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.F5 && ViewModel != null)
                {
                    ViewModel.LoadDataCommand.Execute(null);
                }
            };

            // Obsługa klawisza Delete
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Delete && ViewModel != null && ViewModel.SelectedLaptop != null)
                {
                    ViewModel.DeleteLaptopCommand.Execute(null);
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
                if (ViewModel != null && ViewModel.IsQuickModeActive)
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

        private void FilterKodKreskowy_KeyDown(object sender, KeyEventArgs e)
        {
            // Obsługa klawisza Enter w polu filtrowania kodu kreskowego
            if (e.Key == Key.Enter && ViewModel != null)
            {
                // W trybie szybkim, Enter oznacza zatwierdzenie kodu i zwiększenie liczby sztuk
                if (ViewModel.IsQuickModeActive)
                {
                    ViewModel.ProcessBarcodeInQuickMode();
                    e.Handled = true;
                }
                // W normalnym trybie, Enter powoduje wyszukiwanie
                else
                {
                    ViewModel.SearchByBarcode();
                    e.Handled = true;
                }
            }
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.SelectedLaptop != null;
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.DeleteLaptopCommand.Execute(null);
            }
        }
    }
}