using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs;
using UserManagementService.Interfaces;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Authenticate a user and receive a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto loginDto)
        {
            _logger.LogInformation("Login attempt - UserId: {UserId}", loginDto.Id);
            try
            {
                if (string.IsNullOrWhiteSpace(loginDto.Id) || string.IsNullOrWhiteSpace(loginDto.Password))
                    return BadRequest(new { message = "Id and password are required!" });

                var result = await _userService.Authenticate(loginDto.Id, loginDto.Password);
                if (result == null)
                {
                    _logger.LogWarning("Login failed - Invalid credentials - UserId: {UserId}", loginDto.Id);
                    return Unauthorized(new { message = "Invalid id or password!" });
                }

                _logger.LogInformation("Login successful - UserId: {UserId}", result.User.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error - UserId: {UserId}", loginDto.Id);
                return StatusCode(500, new { message = "An error occurred during login" });
            }
        }

        /// <summary>
        /// Register a new user. Also auto-creates an Employee profile.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDto>> Register([FromBody] UserCreateDto createDto)
        {
            _logger.LogInformation("Registration attempt - UserId: {UserId}", createDto.Id);
            try
            {
                var user = await _userService.CreateUser(createDto);
                if (user == null)
                {
                    _logger.LogWarning("Registration failed - User already exists - UserId: {UserId}", createDto.Id);
                    return BadRequest(new { message = $"User with ID {createDto.Id} already exists!" });
                }

                _logger.LogInformation("Registration successful - UserId: {UserId}", user.Id);
                return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Registration validation error - UserId: {UserId}", createDto.Id);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error - UserId: {UserId}", createDto.Id);
                return StatusCode(500, new { message = "An error occurred during registration" });
            }
        }
    }
}
