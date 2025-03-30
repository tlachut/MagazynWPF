using MahApps.Metro.Controls;
using MagazynLaptopowWPF.ViewModels; // Potrzebne dla DataContext
using System.Windows;
using System.Windows.Input; // Potrzebne dla CommandBinding

namespace MagazynLaptopowWPF
{
    public partial class MainWindow : MetroWindow
    {
        // Pobierz ViewModel dla łatwego dostępu w code-behind (jeśli potrzebne)
        private MainViewModel? ViewModel => DataContext as MainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            // DataContext jest już ustawiony w XAML
        }

        // Obsługa CanExecute dla komendy Delete z DataGrid
        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // Sprawdź, czy komenda usuwania z ViewModelu może być wykonana
            e.CanExecute = ViewModel?.DeleteLaptopCommand.CanExecute(null) ?? false;
        }

        // Obsługa Executed dla komendy Delete z DataGrid
        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // Wywołaj komendę usuwania z ViewModelu
            ViewModel?.DeleteLaptopCommand.Execute(null);
        }
    }
}