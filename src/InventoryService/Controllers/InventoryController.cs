using InventoryService.DTOs;
using InventoryService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryResponseDto>>> GetAllInventory()
        {
            var inventory = await _inventoryService.GetAllInventoryAsync();
            return Ok(inventory);
        }

        [HttpGet("course/{courseId:guid}")]
        public async Task<ActionResult<InventoryResponseDto>> GetInventoryByCourseId(Guid courseId)
        {
            var inventory = await _inventoryService.GetInventoryByCourseIdAsync(courseId);
            if (inventory == null)
                return NotFound(new { message = $"Inventory for course {courseId} was not found." });

            return Ok(inventory);
        }

        [HttpPost]
        public async Task<ActionResult<InventoryResponseDto>> CreateInventory([FromBody] CreateInventoryDto dto)
        {
            try
            {
                var inventory = await _inventoryService.CreateInventoryAsync(dto);
                if (inventory == null)
                    return BadRequest(new { message = "Inventory already exists for this course." });

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("capacity")]
        public async Task<ActionResult<InventoryResponseDto>> UpdateCapacity([FromBody] UpdateCapacityDto dto)
        {
            try
            {
                var inventory = await _inventoryService.UpdateCapacityAsync(dto);
                if (inventory == null)
                    return NotFound(new { message = $"Inventory for course {dto.CourseId} was not found." });

                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("reserve")]
        public async Task<ActionResult> ReserveSeat([FromBody] SeatOperationDto dto)
        {
            var result = await _inventoryService.ReserveSeatAsync(dto);
            if (!result)
                return BadRequest(new { message = "Could not reserve a seat." });

            return Ok(new { message = "Seat reserved successfully." });
        }

        [HttpPost("release")]
        public async Task<ActionResult> ReleaseSeat([FromBody] SeatOperationDto dto)
        {
            var result = await _inventoryService.ReleaseSeatAsync(dto);
            if (!result)
                return BadRequest(new { message = "Could not release a seat." });

            return Ok(new { message = "Seat released successfully." });
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeleteInventory(Guid id)
        {
            var result = await _inventoryService.DeleteInventoryAsync(id);
            if (!result)
                return NotFound(new { message = $"Inventory with ID {id} was not found." });

            return Ok(new { message = "Inventory deleted successfully." });
        }
    }
}
