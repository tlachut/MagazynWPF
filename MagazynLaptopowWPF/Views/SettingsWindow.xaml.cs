using System.Windows;
using MagazynLaptopowWPF.Models;

namespace MagazynLaptopowWPF.Views
{
    public partial class SettingsWindow
    {
        private readonly Settings _settings;

        public SettingsWindow(Settings settings)
        {
            InitializeComponent();
            _settings = settings;
            DataContext = _settings;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            _settings.SaveSettings();
            DialogResult = true;
        }
    }
}