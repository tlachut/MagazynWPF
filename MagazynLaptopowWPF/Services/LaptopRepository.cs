using MagazynLaptopowWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagazynLaptopowWPF.Services
{
    // Interfejs
    public interface ILaptopRepository
    {
        Task<List<Laptop>> GetAllLaptopsAsync();
        Task<Laptop?> GetLaptopByIdAsync(int id);
        Task<Laptop?> GetLaptopByBarcodeAsync(string barcode);
        Task AddLaptopAsync(Laptop laptop);
        Task UpdateLaptopAsync(Laptop laptop);
        Task DeleteLaptopAsync(int id);
        Task<List<Laptop>> GetFilteredLaptopsAsync(string barcodeFilter, string markaFilter, string modelFilter);
        Task<bool> LaptopExistsAsync(string marka, string model);
        Task<bool> BarcodeExistsAsync(string barcode);
        Task IncrementLaptopQuantityAsync(string barcode, int amount = 1);
    }

    // Implementacja
    public class LaptopRepository : ILaptopRepository
    {
        private readonly AppDbContext _context;

        public LaptopRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Laptop>> GetAllLaptopsAsync()
        {
            return await _context.Laptopy.AsNoTracking().ToListAsync();
        }

        public async Task<Laptop?> GetLaptopByIdAsync(int id)
        {
            return await _context.Laptopy.FindAsync(id);
        }

        public async Task<Laptop?> GetLaptopByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return null;

            return await _context.Laptopy
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.KodKreskowy == barcode);
        }

        public async Task AddLaptopAsync(Laptop laptop)
        {
            await _context.Laptopy.AddAsync(laptop);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateLaptopAsync(Laptop laptop)
        {
            // Znajdź istniejący laptop
            var existingLaptop = await _context.Laptopy.FindAsync(laptop.Id);
            if (existingLaptop == null)
                throw new Exception($"Nie znaleziono laptopa o ID: {laptop.Id}");

            // Aktualizuj właściwości ręcznie
            existingLaptop.Marka = laptop.Marka;
            existingLaptop.Model = laptop.Model;
            existingLaptop.SystemOperacyjny = laptop.SystemOperacyjny;
            existingLaptop.RozmiarEkranu = laptop.RozmiarEkranu;
            existingLaptop.Ilosc = laptop.Ilosc;
            existingLaptop.KodKreskowy = laptop.KodKreskowy;

            // Zapisz zmiany
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

        public async Task<List<Laptop>> GetFilteredLaptopsAsync(string barcodeFilter, string markaFilter, string modelFilter)
        {
            var query = _context.Laptopy.AsQueryable();

            if (!string.IsNullOrWhiteSpace(barcodeFilter))
            {
                query = query.Where(l => l.KodKreskowy != null && l.KodKreskowy.Contains(barcodeFilter));
            }

            if (!string.IsNullOrWhiteSpace(markaFilter))
            {
                query = query.Where(l => l.Marka.ToLower().Contains(markaFilter.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(modelFilter))
            {
                query = query.Where(l => l.Model.ToLower().Contains(modelFilter.ToLower()));
            }

            return await query.AsNoTracking().ToListAsync();
        }

        // Dodatkowa metoda do sprawdzania duplikatów
        public async Task<bool> LaptopExistsAsync(string marka, string model)
        {
            return await _context.Laptopy
                .AnyAsync(l => l.Marka.ToLower() == marka.ToLower() && l.Model.ToLower() == model.ToLower());
        }

        // Metoda do sprawdzania duplikatów kodów kreskowych
        public async Task<bool> BarcodeExistsAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return false;

            return await _context.Laptopy
                .AnyAsync(l => l.KodKreskowy == barcode);
        }

        // Metoda do zwiększania ilości laptopa po kodzie kreskowym
        public async Task IncrementLaptopQuantityAsync(string barcode, int amount = 1)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new ArgumentException("Kod kreskowy nie może być pusty", nameof(barcode));

            var laptop = await _context.Laptopy
                .FirstOrDefaultAsync(l => l.KodKreskowy == barcode);

            if (laptop == null)
                throw new Exception($"Nie znaleziono laptopa o kodzie kreskowym: {barcode}");

            laptop.Ilosc += amount;
            await _context.SaveChangesAsync();
        }
    }
}