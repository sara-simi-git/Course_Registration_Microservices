using System;

namespace RegistrationService.DTOs
{
    public class EmployeeRegistrationDto
    {
        public int EmployeeId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class CourseRegistrationResponseDto
    {
        public int RegistrationId { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty; // יושלם במידת הצורך ב-Gateway
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;   // יושלם במידת הצורך ב-Gateway
        public DateTime RegistrationDate { get; set; }
        public decimal AvailableCredits { get; set; }
        public int Status { get; set; }
    }
}