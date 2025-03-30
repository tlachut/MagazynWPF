using System.Windows;
// Dodaj using do ViewModelu, jeśli go używasz w code-behind
using MagazynLaptopowWPF.ViewModels;

namespace MagazynLaptopowWPF.Views // <<< UPEWNIJ SIĘ, ŻE JEST TA PRZESTRZEŃ NAZW
{
    /// <summary>
    /// Logika interakcji dla klasy AddEditLaptopWindow.xaml
    /// </summary>
    public partial class AddEditLaptopWindow : Window
    {
        // Kod konstruktora i metod (np. z poprzedniej odpowiedzi)
        private AddEditLaptopViewModel _viewModel;

        public AddEditLaptopWindow(AddEditLaptopViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        // Anuluj jest obsługiwany przez IsCancel="True" w XAML
    }
}