using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MagazynLaptopowWPF.Models
{
    /// <summary>
    /// Konwerter wartości dla typów Boolean, który obsługuje różne reprezentacje wartości logicznych
    /// </summary>
    public class BooleanConverter : ValueConverter<bool, string>
    {
        public BooleanConverter() : base(
            // Konwersja z bool na string
            v => v ? "1" : "0",

            // Konwersja ze string na bool, obsługuje różne formaty
            v => ConvertToBoolean(v),

            // Opcjonalne parametry konwersji
            null)
        {
        }

        private static bool ConvertToBoolean(string value)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            // Obsługa różnych formatów wartości logicznych
            return value.ToLowerInvariant() switch
            {
                "1" => true,
                "true" => true,
                "t" => true,
                "yes" => true,
                "y" => true,
                "on" => true,
                _ => false
            };
        }
    }
}