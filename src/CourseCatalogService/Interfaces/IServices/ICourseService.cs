using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseCatalogService.DTOs;

namespace CourseCatalogService.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseResponseDto>> GetAllCoursesAsync();
        // Task<IEnumerable<CourseResponseDto>> GetAvailableCoursesAsync();
        Task<CourseResponseDto?> GetCourseByIdAsync(Guid id);
        Task<CourseResponseDto?> GetCourseWithEmployeesAsync(Guid id);
        Task<CourseResponseDto> CreateCourseAsync(CreateCourseDto newCourse);
        Task<CourseResponseDto?> UpdateCourseAsync(Guid id, UpdateCourseDto updCourse);
        Task<bool> DeleteCourseAsync(Guid id);
    }
}