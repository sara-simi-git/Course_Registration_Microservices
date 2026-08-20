namespace UserManagementService.Models
{
    public class User
    {
        public string Id { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string Phone { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Employee;

        // Navigation property - owned by this service
        public Employee? Employee { get; set; }
    }
}
