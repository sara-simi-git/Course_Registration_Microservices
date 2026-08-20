using Microsoft.EntityFrameworkCore;
using UserManagementService.Data;
using UserManagementService.Interfaces;
using UserManagementService.Models;

namespace UserManagementService.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly UserManagementContext _context;
        private readonly ILogger<EmployeeRepository> _logger;

        public EmployeeRepository(UserManagementContext context, ILogger<EmployeeRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("DB: Creating employee for UserId: {UserId}", employee.User?.Id);
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<IEnumerable<Employee>> GetEmployeesListAsync()
        {
            _logger.LogInformation("DB: Retrieving all employees");
            return await _context.Employees
                .Include(e => e.User)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            _logger.LogInformation("DB: Fetching employee - EmployeeId: {EmployeeId}", id);
            return await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Employee?> UpdateEmployeeAsync(Employee employee)
        {
            _logger.LogInformation("DB: Updating employee - EmployeeId: {EmployeeId}", employee.Id);
            _context.Entry(employee).State = EntityState.Modified;

            // Also mark the nested User as modified since EmployeeService may update User fields
            if (employee.User != null)
                _context.Entry(employee.User).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(int id)
        {
            _logger.LogInformation("DB: Deleting employee - EmployeeId: {EmployeeId}", id);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return false;

            // Cascade delete is configured in DbContext - removing Employee also removes User
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Employee?> AddCreditsAsync(int employeeId, int creditsToAdd)
        {
            _logger.LogInformation("DB: Adding {Credits} credits to EmployeeId: {EmployeeId}", creditsToAdd, employeeId);
            var employee = await _context.Employees
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.Id == employeeId);

            if (employee == null) return null;

            employee.TotalCredits += creditsToAdd;
            await _context.SaveChangesAsync();
            return employee;
        }
    }
}
