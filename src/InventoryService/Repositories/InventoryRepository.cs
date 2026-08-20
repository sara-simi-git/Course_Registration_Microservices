using InventoryService.Data;
using InventoryService.Interfaces;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly InventoryDbContext _context;

        public InventoryRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<CourseInventory?> GetByIdAsync(Guid id)
        {
            return await _context.CourseInventories.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<CourseInventory?> GetByCourseIdAsync(Guid courseId)
        {
            return await _context.CourseInventories.FirstOrDefaultAsync(x => x.CourseId == courseId);
        }

        public async Task<IEnumerable<CourseInventory>> GetAllAsync()
        {
            return await _context.CourseInventories.ToListAsync();
        }

        public async Task<CourseInventory> CreateAsync(CourseInventory inventory)
        {
            await _context.CourseInventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<CourseInventory?> UpdateAsync(CourseInventory inventory)
        {
            _context.CourseInventories.Update(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var inventory = await _context.CourseInventories.FirstOrDefaultAsync(x => x.Id == id);
            if (inventory == null)
            {
                return false;
            }

            _context.CourseInventories.Remove(inventory);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
