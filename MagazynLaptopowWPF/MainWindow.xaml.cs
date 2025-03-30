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