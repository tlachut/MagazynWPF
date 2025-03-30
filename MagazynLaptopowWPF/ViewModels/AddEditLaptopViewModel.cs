using MagazynLaptopowWPF.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace MagazynLaptopowWPF.ViewModels
{
    public class AddEditLaptopViewModel : BaseViewModel, IDataErrorInfo
    {
        private Laptop _laptop;
        public Laptop Laptop => _laptop;

        // Właściwości do bindowania UI, które notyfikują o zmianach i umożliwiają walidację
        public string Marka
        {
            get => _laptop.Marka;
            set
            {
                if (_laptop.Marka != value)
                {
                    _laptop.Marka = value;
                    OnPropertyChanged();
                    ValidateProperty(nameof(Marka));
                }
            }
        }

        public string Model
        {
            get => _laptop.Model;
            set
            {
                if (_laptop.Model != value)
                {
                    _laptop.Model = value;
                    OnPropertyChanged();
                    ValidateProperty(nameof(Model));
                }
            }
        }

        public string? SystemOperacyjny
        {
            get => _laptop.SystemOperacyjny;
            set
            {
                if (_laptop.SystemOperacyjny != value)
                {
                    _laptop.SystemOperacyjny = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? RozmiarEkranu
        {
            get => _laptop.RozmiarEkranu;
            set
            {
                if (_laptop.RozmiarEkranu != value)
                {
                    _laptop.RozmiarEkranu = value;
                    OnPropertyChanged();
                    ValidateProperty(nameof(RozmiarEkranu));
                }
            }
        }

        public int Ilosc
        {
            get => _laptop.Ilosc;
            set
            {
                if (_laptop.Ilosc != value)
                {
                    _laptop.Ilosc = value;
                    OnPropertyChanged();
                    ValidateProperty(nameof(Ilosc));
                }
            }
        }

        // Tytuł okna
        public string WindowTitle => _laptop.Id == 0 ? "Dodaj nowy laptop" : $"Edycja laptopa: {_laptop.Marka} {_laptop.Model}";

        // Słownik przechowujący błędy walidacji
        private Dictionary<string, string> _errors = new Dictionary<string, string>();

        public AddEditLaptopViewModel(Laptop laptop)
        {
            _laptop = laptop;

            // Wstępna walidacja wszystkich właściwości
            ValidateProperty(nameof(Marka));
            ValidateProperty(nameof(Model));
            ValidateProperty(nameof(RozmiarEkranu));
            ValidateProperty(nameof(Ilosc));
        }

        // Implementacja IDataErrorInfo dla walidacji w UI
        public string Error => string.Empty;

        public string this[string propertyName]
        {
            get
            {
                ValidateProperty(propertyName);
                return _errors.ContainsKey(propertyName) ? _errors[propertyName] : string.Empty;
            }
        }

        // Metoda do walidacji właściwości
        private void ValidateProperty(string propertyName)
        {
            _errors.Remove(propertyName);

            switch (propertyName)
            {
                case nameof(Marka):
                    if (string.IsNullOrWhiteSpace(Marka))
                    {
                        _errors[propertyName] = "Marka jest wymagana.";
                    }
                    else if (Marka.Length > 100)
                    {
                        _errors[propertyName] = "Marka nie może mieć więcej niż 100 znaków.";
                    }
                    break;

                case nameof(Model):
                    if (string.IsNullOrWhiteSpace(Model))
                    {
                        _errors[propertyName] = "Model jest wymagany.";
                    }
                    else if (Model.Length > 100)
                    {
                        _errors[propertyName] = "Model nie może mieć więcej niż 100 znaków.";
                    }
                    break;

                case nameof(RozmiarEkranu):
                    if (RozmiarEkranu.HasValue && (RozmiarEkranu < 0 || RozmiarEkranu > 100))
                    {
                        _errors[propertyName] = "Rozmiar ekranu musi być między 0 a 100 cali.";
                    }
                    break;

                case nameof(Ilosc):
                    if (Ilosc < 0)
                    {
                        _errors[propertyName] = "Ilość nie może być ujemna.";
                    }
                    break;
            }

            OnPropertyChanged(nameof(IsValid));
        }

        // Właściwość do sprawdzania, czy wszystkie dane są poprawne
        public bool IsValid => !_errors.Any();
    }
}