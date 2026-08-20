using System;
using RegistrationService.Enums;

namespace RegistrationService.Models
{
    public class CourseRegistration
    {
        public int Id { get; set; }

        public int EmployeeId { get; set; } 
        
        public Guid CourseId { get; set; }   
        
        public DateTime RegistrationDate { get; set; }
        
        public CourseRegistrationStatus Status { get; set; } = CourseRegistrationStatus.Active;
    }
}