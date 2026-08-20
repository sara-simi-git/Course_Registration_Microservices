using UserManagementService.DTOs;
using UserManagementService.Interfaces;
using UserManagementService.Models;

namespace UserManagementService.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository employeeRepository, ILogger<EmployeeService> logger)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetEmployeesListAsync()
        {
            var employees = await _employeeRepository.GetEmployeesListAsync();
            return employees.Select(MapToResponseDto);
        }

        public async Task<EmployeeResponseDto?> GetEmployeeByIdAsync(int id)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            return employee != null ? MapToResponseDto(employee) : null;
        }

        public async Task<EmployeeResponseDto?> UpdateEmployee(int id, EmployeeUpdateDto updateDto)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(id);
            if (employee == null) return null;

            // Update User fields
            if (!string.IsNullOrWhiteSpace(updateDto.FirstName)) employee.User.FirstName = updateDto.FirstName;
            if (!string.IsNullOrWhiteSpace(updateDto.LastName)) employee.User.LastName = updateDto.LastName;
            if (updateDto.Email != null) employee.User.Email = updateDto.Email;
            if (updateDto.Phone != null) employee.User.Phone = updateDto.Phone;

            // Update Employee fields
            if (!string.IsNullOrWhiteSpace(updateDto.Department)) employee.Department = updateDto.Department;
            if (updateDto.TotalCredits.HasValue) employee.TotalCredits = updateDto.TotalCredits.Value;

            var updated = await _employeeRepository.UpdateEmployeeAsync(employee);
            return updated != null ? MapToResponseDto(updated) : null;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            return await _employeeRepository.DeleteEmployeeAsync(id);
        }

        public async Task<EmployeeResponseDto?> AddCreditsAsync(int employeeId, int creditsToAdd)
        {
            if (creditsToAdd <= 0)
            {
                _logger.LogWarning("AddCredits: Invalid amount {Credits} for EmployeeId: {Id}", creditsToAdd, employeeId);
                return null;
            }

            var updated = await _employeeRepository.AddCreditsAsync(employeeId, creditsToAdd);
            return updated != null ? MapToResponseDto(updated) : null;
        }

        // ── Private helpers ────────────────────────────────────────────────────
        private static EmployeeResponseDto MapToResponseDto(Employee emp) => new()
        {
            Id = emp.User.Id,
            FirstName = emp.User.FirstName,
            LastName = emp.User.LastName,
            Email = emp.User.Email,
            Phone = emp.User.Phone,
            Role = emp.User.Role,
            EmployeeId = emp.Id,
            Department = emp.Department,
            TotalCredits = emp.TotalCredits,
            UsedCredits = emp.UsedCredits,
            AvailableCredits = emp.AvailableCredits
        };
    }
}
