using System.Windows;
using MagazynLaptopowWPF.ViewModels;

namespace MagazynLaptopowWPF.Views
{
    /// <summary>
    /// Logika interakcji dla klasy AddEditLaptopWindow.xaml
    /// </summary>
    public partial class AddEditLaptopWindow
    {
        private readonly AddEditLaptopViewModel _viewModel;

        public AddEditLaptopWindow(AddEditLaptopViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;

            // Zapobiega zamknięciu okna, gdy naciśnięty jest Enter w TextBoxie
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter && !(e.OriginalSource is System.Windows.Controls.Button))
                {
                    // Jeśli naciśnięto Enter nie w przycisku, zablokuj domyślną akcję
                    e.Handled = true;

                    // Opcjonalnie: jeśli wszystkie dane są poprawne, zamknij okno dialogowe z rezultatem true
                    if (_viewModel.IsValid)
                    {
                        DialogResult = true;
                    }
                }
            };

            // Po załadowaniu okna, ustawienie fokusu na pole kodu kreskowego
            Loaded += (s, e) =>
            {
                txtKodKreskowy.Focus();
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.IsValid)
            {
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Formularz zawiera błędy. Proszę poprawić zaznaczone pola.",
                                "Błędy walidacji", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}