// Plik: ViewModels/AddEditLaptopViewModel.cs
using MagazynLaptopowWPF.Models; // Potrzebne dla Laptop

namespace MagazynLaptopowWPF.ViewModels
{
    // Możesz dziedziczyć z BaseViewModel, jeśli chcesz używać OnPropertyChanged
    public class AddEditLaptopViewModel : BaseViewModel
    {
        private Laptop _laptop;
        public Laptop Laptop
        {
            get => _laptop;
            // Lepiej nie pozwalać na ustawienie całego obiektu z zewnątrz po inicjalizacji
            // set => SetProperty(ref _laptop, value);
        }

        // Komendy np. Save, Cancel (na razie puste lub null)
        // public ICommand SaveCommand { get; }
        // public ICommand CancelCommand { get; }

        public AddEditLaptopViewModel(Laptop laptop)
        {
            _laptop = laptop; // Przechowujemy referencję do edytowanego/nowego laptopa
        }

        // Można dodać logikę walidacji (np. IDataErrorInfo)
    }
}