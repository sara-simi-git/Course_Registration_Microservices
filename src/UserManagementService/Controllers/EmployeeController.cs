using Microsoft.AspNetCore.Mvc;
using UserManagementService.DTOs;
using UserManagementService.Interfaces;

namespace UserManagementService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpGet("employeesList")]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployeesList()
        {
            _logger.LogInformation("Fetching all employees");
            var employees = await _employeeService.GetEmployeesListAsync();
            return Ok(employees);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id)
        {
            _logger.LogInformation("Fetching employee - EmployeeId: {EmployeeId}", id);
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} does not exist!" });
            return Ok(employee);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto updateDto)
        {
            _logger.LogInformation("Updating employee - EmployeeId: {EmployeeId}", id);
            try
            {
                var updated = await _employeeService.UpdateEmployee(id, updateDto);
                if (updated == null)
                    return NotFound(new { message = $"Employee with ID {id} does not exist!" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee - EmployeeId: {EmployeeId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteEmployee(int id)
        {
            _logger.LogInformation("Deleting employee - EmployeeId: {EmployeeId}", id);
            var deleted = await _employeeService.DeleteEmployeeAsync(id);
            if (!deleted)
                return NotFound(new { message = $"Employee with ID {id} does not exist!" });
            return Ok("Deleted!");
        }

        [HttpPost("{employeeId}/addCredits")]
        public async Task<ActionResult<EmployeeResponseDto>> AddCredits(int employeeId, [FromBody] int amount)
        {
            _logger.LogInformation("Adding credits to EmployeeId: {EmployeeId}", employeeId);
            try
            {
                var updated = await _employeeService.AddCreditsAsync(employeeId, amount);
                if (updated == null)
                    return BadRequest(new { message = $"Invalid credits amount or Employee with ID {employeeId} does not exist!" });
                return Ok(updated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding credits - EmployeeId: {EmployeeId}", employeeId);
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
