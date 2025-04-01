using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace MagazynLaptopowWPF.Models
{
    public class Settings : INotifyPropertyChanged
    {
        private static string SettingsPath => Path.Combine(
            Directory.GetCurrentDirectory(), "AppSettings.json");

        private bool _showBarcodes = true;

        public bool ShowBarcodes
        {
            get => _showBarcodes;
            set
            {
                if (_showBarcodes != value)
                {
                    _showBarcodes = value;
                    OnPropertyChanged();
                    SaveSettings();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Zapisywanie i odczytywanie ustawień
        public void SaveSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
                // Obsługa błędów zapisu
            }
        }

        public static Settings LoadSettings()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    string json = File.ReadAllText(SettingsPath);
                    var settings = JsonSerializer.Deserialize<Settings>(json);
                    return settings ?? new Settings();
                }
            }
            catch
            {
                // Obsługa błędów odczytu
            }

            return new Settings();
        }
    }
}