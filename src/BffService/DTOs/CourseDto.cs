using System;

namespace BffService.DTOs
{
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int CreditCost { get; set; }
        public Guid? LecturerId { get; set; }
        public string LecturerName { get; set; } = string.Empty;
    }
}
