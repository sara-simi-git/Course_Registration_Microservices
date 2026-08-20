using System.Collections.Generic;
using System.Threading.Tasks;
using RegistrationService.DTOs;

namespace RegistrationService.Interfaces
{
    public interface IRegistrationService
    {
        Task<CourseRegistrationResponseDto?> GetRegistrationByIdAsync(int id);
        Task<IEnumerable<CourseRegistrationResponseDto>> GetEmployeeRegistrationsAsync(int employeeId);
        Task<IEnumerable<CourseRegistrationResponseDto>> GetCourseRegistrationsAsync(Guid courseId);
        Task<CourseRegistrationResponseDto?> RegisterToCourseAsync(EmployeeRegistrationDto dto, string? correlationId = null);
        Task<bool> ChangeRegistrationStatusAsync(int registrationId, int status);
        Task<bool> CancelRegistrationAsync(int registrationId);
    }
}