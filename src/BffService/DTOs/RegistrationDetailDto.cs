using System;

namespace BffService.DTOs
{
    public class RegistrationDetailDto
    {
        public int RegistrationId { get; set; }
        public int EmployeeId { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseDescription { get; set; } = string.Empty;
        public int CreditCost { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public DateTime RegistrationDate { get; set; }
        public decimal AvailableCredits { get; set; }
        public int Status { get; set; }
    }
}
