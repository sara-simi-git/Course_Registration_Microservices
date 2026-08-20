using UserManagementService.Models;

namespace UserManagementService.DTOs
{
    public class EmployeeResponseDto
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        public int EmployeeId { get; set; }
        public string Department { get; set; } = string.Empty;
        public int TotalCredits { get; set; }
        public int UsedCredits { get; set; }
        public int AvailableCredits { get; set; }
    }

    public class EmployeeUpdateDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Department { get; set; }
        public int? TotalCredits { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string Department { get; set; } = string.Empty;
        public int TotalCredits { get; set; }
    }
}
