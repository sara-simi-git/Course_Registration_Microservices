using System;

namespace BffService.DTOs
{
    public class RegistrationDto
    {
        public int RegistrationId { get; set; }
        public int EmployeeId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public decimal AvailableCredits { get; set; }
        public int Status { get; set; }
    }
}
