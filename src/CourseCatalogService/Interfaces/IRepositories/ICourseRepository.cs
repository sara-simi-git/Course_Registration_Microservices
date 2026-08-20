using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseCatalogService.Models;

namespace CourseCatalogService.Interfaces
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        // Task<IEnumerable<Course>> GetAvailableCoursesAsync();
        Task<Course?> GetCourseByIdAsync(Guid id);
        Task<Course?> GetCourseWithEmployeesAsync(Guid id);
        Task<Course> CreateCourseAsync(Course course);
        Task<Course?> UpdateCourseAsync(Course course);
        Task<bool> UpdateLecturerNameForCoursesAsync(Guid lecturerId, string lecturerName);
        Task<bool> DeleteCourseAsync(Guid id);
    }
}