using InventoryService.Models;

namespace InventoryService.Interfaces
{
    public interface IInventoryRepository
    {
        Task<CourseInventory?> GetByIdAsync(Guid id);
        Task<CourseInventory?> GetByCourseIdAsync(Guid courseId);
        Task<IEnumerable<CourseInventory>> GetAllAsync();
        Task<CourseInventory> CreateAsync(CourseInventory inventory);
        Task<CourseInventory?> UpdateAsync(CourseInventory inventory);
        Task<bool> DeleteAsync(Guid id);
        Task SaveChangesAsync();
    }
}
