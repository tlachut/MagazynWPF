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
        Task AddLaptopAsync(Laptop laptop);
        Task UpdateLaptopAsync(Laptop laptop);
        Task DeleteLaptopAsync(int id);
        Task<List<Laptop>> GetFilteredLaptopsAsync(string markaFilter, string modelFilter);
        Task<bool> LaptopExistsAsync(string marka, string model);
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

        public async Task<List<Laptop>> GetFilteredLaptopsAsync(string markaFilter, string modelFilter)
        {
            var query = _context.Laptopy.AsQueryable();

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
    }
}