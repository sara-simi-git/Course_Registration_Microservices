using System.ComponentModel.DataAnnotations;
using UserManagementService.Models;

namespace UserManagementService.DTOs
{
    public class UserResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }

    public class UserCreateDto
    {
        [Required, RegularExpression(@"^\d{9}$", ErrorMessage = "Id must have 9 digits!")]
        public string Id { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, Phone]
        public string Phone { get; set; } = string.Empty;

        [Required, MinLength(8, ErrorMessage = "Password must have at least 8 characters!")]
        public string Password { get; set; } = string.Empty;
    }

    public class UserUpdateDto
    {
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MaxLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Phone]
        public string? Phone { get; set; }
    }
}
