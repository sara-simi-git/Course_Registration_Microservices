using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs;
using UserManagementService.Interfaces;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("usersList")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsersList()
        {
            _logger.LogInformation("Fetching all users");
            try
            {
                var users = await _userService.GetUsersList();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users list");
                return StatusCode(500, new { message = "An error occurred while fetching users" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(string id)
        {
            _logger.LogInformation("Fetching user - UserId: {UserId}", id);
            try
            {
                var user = await _userService.GetUserById(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found - UserId: {UserId}", id);
                    return NotFound(new { message = $"User with ID {id} does not exist!" });
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching user - UserId: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while fetching user" });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] UserCreateDto createDto)
        {
            _logger.LogInformation("Creating user - UserId: {UserId}", createDto.Id);
            try
            {
                var created = await _userService.CreateUser(createDto);
                if (created == null)
                {
                    _logger.LogWarning("User already exists - UserId: {UserId}", createDto.Id);
                    return BadRequest(new { message = $"User with ID {createDto.Id} already exists!" });
                }
                _logger.LogInformation("User created - UserId: {UserId}", created.Id);
                return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user - UserId: {UserId}", createDto.Id);
                return StatusCode(500, new { message = "An error occurred while creating user" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(string id, [FromBody] UserUpdateDto updateDto)
        {
            _logger.LogInformation("Updating user - UserId: {UserId}", id);
            try
            {
                var updated = await _userService.UpdateUser(id, updateDto);
                if (updated == null)
                {
                    _logger.LogWarning("User not found for update - UserId: {UserId}", id);
                    return NotFound(new { message = $"User with ID {id} does not exist!" });
                }
                _logger.LogInformation("User updated - UserId: {UserId}", id);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user - UserId: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while updating user" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id)
        {
            _logger.LogInformation("Deleting user - UserId: {UserId}", id);
            try
            {
                var deleted = await _userService.DeleteUser(id);
                if (!deleted)
                {
                    _logger.LogWarning("User not found for delete - UserId: {UserId}", id);
                    return NotFound(new { message = $"User with ID {id} does not exist!" });
                }
                _logger.LogInformation("User deleted - UserId: {UserId}", id);
                return Ok("Deleted!");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user - UserId: {UserId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting user" });
            }
        }
    }
}
