using UserManagementService.Models;

namespace UserManagementService.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<IEnumerable<Employee>> GetEmployeesListAsync();
        Task<Employee?> GetEmployeeByIdAsync(int id);
        Task<Employee?> UpdateEmployeeAsync(Employee employee);
        Task<bool> DeleteEmployeeAsync(int id);
        Task<Employee?> AddCreditsAsync(int employeeId, int creditsToAdd);
    }
}
