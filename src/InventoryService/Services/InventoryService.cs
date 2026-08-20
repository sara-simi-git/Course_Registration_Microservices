using InventoryService.DTOs;
using InventoryService.Interfaces;
using InventoryService.Models;

namespace InventoryService.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<InventoryResponseDto?> CreateInventoryAsync(CreateInventoryDto dto)
        {
            var existing = await _inventoryRepository.GetByCourseIdAsync(dto.CourseId);
            if (existing != null)
            {
                return null;
            }

            var inventory = new CourseInventory
            {
                Id = Guid.NewGuid(),
                CourseId = dto.CourseId,
                MaxPlaces = dto.MaxPlaces,
                ReservedPlaces = 0
            };

            var created = await _inventoryRepository.CreateAsync(inventory);
            return MapToResponseDto(created);
        }

        public async Task<InventoryResponseDto?> GetInventoryByCourseIdAsync(Guid courseId)
        {
            var inventory = await _inventoryRepository.GetByCourseIdAsync(courseId);
            return inventory != null ? MapToResponseDto(inventory) : null;
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync()
        {
            var inventoryItems = await _inventoryRepository.GetAllAsync();
            return inventoryItems.Select(MapToResponseDto);
        }

        public async Task<InventoryResponseDto?> UpdateCapacityAsync(UpdateCapacityDto dto)
        {
            var inventory = await _inventoryRepository.GetByCourseIdAsync(dto.CourseId);
            if (inventory == null)
            {
                return null;
            }

            inventory.MaxPlaces = dto.MaxPlaces;
            var updated = await _inventoryRepository.UpdateAsync(inventory);
            return updated != null ? MapToResponseDto(updated) : null;
        }

        public async Task<bool> ReserveSeatAsync(SeatOperationDto dto)
        {
            var inventory = await _inventoryRepository.GetByCourseIdAsync(dto.CourseId);
            if (inventory == null)
            {
                return false;
            }

            if (inventory.ReservedPlaces >= inventory.MaxPlaces)
            {
                return false;
            }

            inventory.ReservedPlaces++;
            await _inventoryRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReleaseSeatAsync(SeatOperationDto dto)
        {
            var inventory = await _inventoryRepository.GetByCourseIdAsync(dto.CourseId);
            if (inventory == null)
            {
                return false;
            }

            if (inventory.ReservedPlaces > 0)
            {
                inventory.ReservedPlaces--;
                await _inventoryRepository.SaveChangesAsync();
            }

            return true;
        }

        public async Task<bool> DeleteInventoryAsync(Guid id)
        {
            return await _inventoryRepository.DeleteAsync(id);
        }

        private static InventoryResponseDto MapToResponseDto(CourseInventory inventory)
        {
            return new InventoryResponseDto
            {
                Id = inventory.Id,
                CourseId = inventory.CourseId,
                MaxPlaces = inventory.MaxPlaces,
                ReservedPlaces = inventory.ReservedPlaces,
                PlacesLeft = inventory.PlacesLeft
            };
        }
    }
}
