using Microsoft.AspNetCore.Mvc;
using NotificationService.DTOs;
using NotificationService.Interfaces.IServices;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;

    public NotificationController(INotificationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Send([FromBody] CreateNotificationDto dto)
    {
        var result = await _service.SendNotificationAsync(dto);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var result = await _service.GetByUserIdAsync(userId);
        return Ok(result);
    }

    [HttpPost("{id}/retry")]
    public async Task<IActionResult> Retry(string id)
    {
        await _service.RetryFailedAsync(id);
        return NoContent();
    }
}
