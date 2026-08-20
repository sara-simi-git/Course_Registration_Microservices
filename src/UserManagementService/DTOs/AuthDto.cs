using System.ComponentModel.DataAnnotations;

namespace UserManagementService.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
        public UserResponseDto User { get; set; } = null!;
    }
}
