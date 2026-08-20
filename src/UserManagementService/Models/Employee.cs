namespace UserManagementService.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public string Department { get; set; } = string.Empty;
        public int TotalCredits { get; set; }
        public int UsedCredits { get; set; }

        // Computed - not stored in DB
        public int AvailableCredits => TotalCredits - UsedCredits;
    }
}
