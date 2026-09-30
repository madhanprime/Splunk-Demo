using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Models;
using Serilog;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/employees
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees([FromQuery] string? search, [FromQuery] string? department)
        {
            Log.ForContext("Application", "EmployeeApp")
               .ForContext("QuerySearch", search)
               .ForContext("QueryDept", department)
               .Information("Fetching employee list with filters Search={Search}, Department={Dept}", search, department);

            var query = _context.Employees.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e => e.FirstName.Contains(search) || e.LastName.Contains(search) || e.Email.Contains(search));
            }

            if (!string.IsNullOrEmpty(department))
            {
                query = query.Where(e => e.Department == department);
            }

            return await query.ToListAsync();
        }

        // GET: api/employees/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                Log.ForContext("Application", "EmployeeApp")
                   .ForContext("EmployeeId", id)
                   .Warning("Employee with ID {Id} was not found", id);

                return NotFound(new { Message = $"Employee with ID {id} not found." });
            }

            Log.ForContext("Application", "EmployeeApp")
               .ForContext("EmployeeId", id)
               .Information("Successfully retrieved employee {Id} ({FirstName} {LastName})", id, employee.FirstName, employee.LastName);

            return employee;
        }

        // POST: api/employees
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee([FromBody] Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Email) || !employee.Email.Contains("@"))
            {
                Log.ForContext("Application", "EmployeeApp")
                   .ForContext("Email", employee.Email)
                   .Warning("Validation failed: Invalid email format '{Email}'", employee.Email);

                return BadRequest(new { Message = "Valid email is required." });
            }

            employee.CreatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            Log.ForContext("Application", "EmployeeApp")
               .ForContext("EmployeeId", employee.Id)
               .ForContext("Department", employee.Department)
               .Information("Created new employee ID {Id} in department {Department}", employee.Id, employee.Department);

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
        }

        // PUT: api/employees/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] Employee employee)
        {
            if (id != employee.Id)
            {
                return BadRequest(new { Message = "ID mismatch." });
            }

            var existing = await _context.Employees.FindAsync(id);
            if (existing == null)
            {
                Log.ForContext("Application", "EmployeeApp")
                   .ForContext("EmployeeId", id)
                   .Warning("Failed update: Employee {Id} not found", id);

                return NotFound();
            }

            existing.FirstName = employee.FirstName;
            existing.LastName = employee.LastName;
            existing.Email = employee.Email;
            existing.Department = employee.Department;
            existing.Salary = employee.Salary;
            existing.Role = employee.Role;

            await _context.SaveChangesAsync();

            Log.ForContext("Application", "EmployeeApp")
               .ForContext("EmployeeId", id)
               .Information("Updated employee {Id}", id);

            return NoContent();
        }

        // DELETE: api/employees/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            Log.ForContext("Application", "EmployeeApp")
               .ForContext("EmployeeId", id)
               .Information("Deleted employee {Id}", id);

            return NoContent();
        }

        // DEMO ENDPOINT: Simulate Telemetry Anomaly (Slow responses, 500 exceptions, DB timeout)
        [HttpGet("simulate/{type}")]
        public async Task<IActionResult> SimulateAnomaly(string type)
        {
            switch (type.ToLower())
            {
                case "slow":
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("AnomalyType", "SlowResponse")
                       .Warning("Simulating slow API database query execution delay...");
                    await Task.Delay(2500); // 2.5 sec delay
                    return Ok(new { Message = "Slow API response completed in 2500ms" });

                case "error500":
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("AnomalyType", "UnhandledException")
                       .Error("Simulating 500 Internal Server Error in Employee Payroll Calculation Service");
                    throw new InvalidOperationException("Fatal: Database Connection Timeout while executing stored procedure 'sp_CalculateMonthlyPayroll'");

                case "db-timeout":
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("AnomalyType", "DbTimeout")
                       .Error("Simulating Database Connection Timeout Exception");
                    return StatusCode(500, new { Error = "Database Timeout Error", Code = "DB_TIMEOUT_504" });

                case "unauthorized":
                    Log.ForContext("Application", "EmployeeApp")
                       .ForContext("AnomalyType", "UnauthorizedAccess")
                       .Warning("Unauthorized access attempt to confidential salary endpoint");
                    return StatusCode(401, new { Error = "Unauthorized access", Code = "AUTH_401" });

                default:
                    return BadRequest(new { Message = "Unknown anomaly type. Use: slow, error500, db-timeout, unauthorized" });
            }
        }
    }
}
