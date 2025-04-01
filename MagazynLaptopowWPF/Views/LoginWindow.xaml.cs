using System.ComponentModel;
using System.Windows;
using MagazynLaptopowWPF.ViewModels; // Upewnij się, że ten using jest

namespace MagazynLaptopowWPF.Views
{
    /// <summary>
    /// Logika interakcji dla klasy LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow
    {
        // Pobierz ViewModel z DataContext
        private LoginViewModel? ViewModel => DataContext as LoginViewModel;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Rozpocznij monitorowanie po załadowaniu okna
            ViewModel?.InitializeMonitoring();
            // Subskrybuj zdarzenie sukcesu logowania
            if (ViewModel != null)
            {
                ViewModel.LoginSuccess += ViewModel_LoginSuccess;
            }
        }

        private void ViewModel_LoginSuccess(object? sender, LoginSuccessEventArgs e)
        {
            // Logowanie udane, zamknij to okno i otwórz główne
            var mainViewModel = new MainViewModel(e.UserName); // Przekaż nazwę użytkownika
            var mainWindow = new MainWindow
            {
                DataContext = mainViewModel
            };

            // Ustaw nowe okno jako główne okno aplikacji
            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();
            this.Close(); // Zamknij okno logowania
        }

        private void LoginWindow_Closing(object sender, CancelEventArgs e)
        {
            // Zwolnij zasoby ViewModelu przy zamykaniu okna
            // Odsubskrybuj zdarzenie, aby uniknąć wycieków pamięci
            if (ViewModel != null)
            {
                ViewModel.LoginSuccess -= ViewModel_LoginSuccess;
                ViewModel.Cleanup(); // Użyj metody Cleanup
            }
        }
    }
}