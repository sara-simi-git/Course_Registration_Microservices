using UserManagementService.DTOs;
using UserManagementService.Interfaces;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IUserRepository userRepository,
            IEmployeeRepository employeeRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _userRepository = userRepository;
            _employeeRepository = employeeRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsersList()
        {
            var users = await _userRepository.GetUsersList();
            return users.Select(MapToResponseDto);
        }

        public async Task<UserResponseDto?> GetUserById(string id)
        {
            var user = await _userRepository.GetUserById(id);
            return user != null ? MapToResponseDto(user) : null;
        }

        public async Task<LoginResponseDto?> Authenticate(string id, string password)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                _logger.LogWarning("Auth failed: User not found - UserId: {UserId}", id);
                return null;
            }

            if (user.Password != HashPassword(password))
            {
                _logger.LogWarning("Auth failed: Invalid password - UserId: {UserId}", id);
                return null;
            }

            var token = _tokenService.GenerateToken(
                user.Id, user.Email, $"{user.FirstName} {user.LastName}", user.Role);

            var expiryMinutes = _configuration.GetValue<int>("JwtSettings:ExpiryMinutes", 60);

            _logger.LogInformation("Authentication successful - UserId: {UserId}", id);

            return new LoginResponseDto
            {
                Token = token,
                TokenType = "Bearer",
                ExpiresIn = expiryMinutes * 60,
                User = MapToResponseDto(user)
            };
        }

        public async Task<UserResponseDto?> CreateUser(UserCreateDto createDto)
        {
            var existing = await _userRepository.GetUserById(createDto.Id);
            if (existing != null)
            {
                _logger.LogWarning("CreateUser: User already exists - UserId: {UserId}", createDto.Id);
                return null;
            }

            var newUser = new User
            {
                Id = createDto.Id,
                FirstName = createDto.FirstName,
                LastName = createDto.LastName,
                Email = createDto.Email,
                Phone = createDto.Phone,
                Role = UserRole.Employee,
                Password = HashPassword(createDto.Password)
            };

            var createdUser = await _userRepository.CreateUser(newUser);

            // Auto-create an Employee profile for every new user
            var newEmployee = new Employee
            {
                User = createdUser,
                Department = "General",
                TotalCredits = 20,
                UsedCredits = 0
            };
            await _employeeRepository.CreateEmployeeAsync(newEmployee);

            _logger.LogInformation("User and Employee profile created - UserId: {UserId}", createdUser.Id);
            return MapToResponseDto(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUser(string id, UserUpdateDto updateDto)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null) return null;

            if (!string.IsNullOrWhiteSpace(updateDto.FirstName)) user.FirstName = updateDto.FirstName;
            if (!string.IsNullOrWhiteSpace(updateDto.LastName)) user.LastName = updateDto.LastName;
            if (updateDto.Email != null) user.Email = updateDto.Email;
            if (updateDto.Phone != null) user.Phone = updateDto.Phone;

            var updated = await _userRepository.UpdateUser(user);
            return updated != null ? MapToResponseDto(updated) : null;
        }

        public async Task<bool> DeleteUser(string id)
        {
            return await _userRepository.DeleteUser(id);
        }

        // ── Private helpers ────────────────────────────────────────────────────
        private static UserResponseDto MapToResponseDto(User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role
        };

        private static string HashPassword(string password) =>
            Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
    }
}
