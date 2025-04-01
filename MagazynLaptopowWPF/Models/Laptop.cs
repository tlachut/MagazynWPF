using System.ComponentModel.DataAnnotations;

namespace MagazynLaptopowWPF.Models
{
    public class Laptop
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Marka jest wymagana.")]
        [MaxLength(100)]
        public string Marka { get; set; } = string.Empty; // Inicjalizacja

        [Required(ErrorMessage = "Model jest wymagany.")]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty; // Inicjalizacja

        [MaxLength(100)]
        public string? SystemOperacyjny { get; set; }

        public double? RozmiarEkranu { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Ilość nie może być ujemna.")]
        public int Ilosc { get; set; }

        [MaxLength(50)]
        public string? KodKreskowy { get; set; }

        public override string ToString()
        {
            return $"{Marka} {Model} ({Ilosc} szt.)";
        }
    }
}