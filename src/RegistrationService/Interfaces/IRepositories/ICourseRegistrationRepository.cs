using System.Collections.Generic;
using System.Threading.Tasks;
using RegistrationService.Enums;
using RegistrationService.Models;

namespace RegistrationService.Interfaces
{
    public interface ICourseRegistrationRepository
    {
        Task<CourseRegistration?> GetByIdAsync(int id);
        Task<CourseRegistration?> GetByEmployeeAndCourseAsync(int employeeId, Guid courseId);
        Task<IEnumerable<CourseRegistration>> GetEmployeesByCourseIdAsync(Guid courseId);
        Task<IEnumerable<CourseRegistration>> GetCoursesByEmployeeIdAsync(int employeeId);
        Task AddRegistrationAsync(CourseRegistration registration);
        Task<CourseRegistrationStatus> ChangeStatusCourseAsync(int registrationId, CourseRegistrationStatus newStatus);
        Task SaveChangesAsync();
    }
}