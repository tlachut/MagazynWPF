using MagazynLaptopowWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Do operacji asynchronicznych

namespace MagazynLaptopowWPF.Services
{
    // Interfejs
    public interface ILaptopRepository
    {
        Task<List<Laptop>> GetAllLaptopsAsync();
        Task<Laptop?> GetLaptopByIdAsync(int id);
        Task AddLaptopAsync(Laptop laptop);
        Task UpdateLaptopAsync(Laptop laptop);
        Task DeleteLaptopAsync(int id);
        Task<List<Laptop>> GetFilteredLaptopsAsync(string markaFilter, string modelFilter); // Dla filtrowania
    }

    // Implementacja
    public class LaptopRepository : ILaptopRepository
    {
        private readonly AppDbContext _context;

        // Wstrzykiwanie zależności (Dependency Injection) - najlepsza praktyka
        // W prostszej wersji można tworzyć kontekst bezpośrednio: new AppDbContext()
        public LaptopRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Laptop>> GetAllLaptopsAsync()
        {
            return await _context.Laptopy.ToListAsync();
        }

        public async Task<Laptop?> GetLaptopByIdAsync(int id)
        {
            return await _context.Laptopy.FindAsync(id);
        }

        public async Task AddLaptopAsync(Laptop laptop)
        {
            await _context.Laptopy.AddAsync(laptop);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLaptopAsync(Laptop laptop)
        {
            _context.Entry(laptop).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteLaptopAsync(int id)
        {
            var laptop = await _context.Laptopy.FindAsync(id);
            if (laptop != null)
            {
                _context.Laptopy.Remove(laptop);
                await _context.SaveChangesAsync();
            }
        }

        // Przykład filtrowania po stronie serwera (bazy danych)
        public async Task<List<Laptop>> GetFilteredLaptopsAsync(string markaFilter, string modelFilter)
        {
            var query = _context.Laptopy.AsQueryable(); // Zaczynamy od całej tabeli

            if (!string.IsNullOrWhiteSpace(markaFilter))
            {
                query = query.Where(l => l.Marka.ToLower().Contains(markaFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(modelFilter))
            {
                query = query.Where(l => l.Model.ToLower().Contains(modelFilter.ToLower()));
            }

            return await query.ToListAsync();
        }
    }
}