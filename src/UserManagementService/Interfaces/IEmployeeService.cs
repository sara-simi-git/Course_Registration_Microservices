using UserManagementService.DTOs;

namespace UserManagementService.Interfaces
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetEmployeesListAsync();
        Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id);
        Task<EmployeeResponseDto?> UpdateEmployee(int id, EmployeeUpdateDto updateDto);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<EmployeeResponseDto?> AddCreditsAsync(int employeeId, int creditsToAdd);
    }
}
