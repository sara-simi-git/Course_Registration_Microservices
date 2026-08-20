using InventoryService.DTOs;

namespace InventoryService.Interfaces
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto?> CreateInventoryAsync(CreateInventoryDto dto);
        Task<InventoryResponseDto?> GetInventoryByCourseIdAsync(Guid courseId);
        Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync();
        Task<InventoryResponseDto?> UpdateCapacityAsync(UpdateCapacityDto dto);
        Task<bool> ReserveSeatAsync(SeatOperationDto dto);
        Task<bool> ReleaseSeatAsync(SeatOperationDto dto);
        Task<bool> DeleteInventoryAsync(Guid id);
    }
}
