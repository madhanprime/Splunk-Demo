using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.Api.Models;
using Serilog;

namespace EmployeeManagement.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, int> FailedAttempts = new();

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

            // Demo mock credential check
            if (request.Username == "admin" && request.Password == "password123")
            {
                FailedAttempts[request.Username] = 0;

                Log.ForContext("Application", "EmployeeApp")
                   .ForContext("EventType", "UserLoginSuccess")
                   .ForContext("Username", request.Username)
                   .ForContext("ClientIp", clientIp)
                   .Information("User '{Username}' logged in successfully", request.Username);

                return Ok(new LoginResponse
                {
                    Success = true,
                    Username = request.Username,
                    Token = "demo-jwt-token-splunk-monitoring-12345",
                    Message = "Login successful"
                });
            }

            // Failed login track
            if (!FailedAttempts.ContainsKey(request.Username))
                FailedAttempts[request.Username] = 0;

            FailedAttempts[request.Username]++;
            var currentFails = FailedAttempts[request.Username];

            Log.ForContext("Application", "EmployeeApp")
               .ForContext("EventType", "UserLoginFailure")
               .ForContext("Username", request.Username)
               .ForContext("FailedAttemptCount", currentFails)
               .ForContext("ClientIp", clientIp)
               .Warning("Failed login attempt #{Count} for user '{Username}' from IP {ClientIp}", currentFails, request.Username, clientIp);

            return Unauthorized(new LoginResponse
            {
                Success = false,
                Username = request.Username,
                Message = "Invalid credentials. Attempt logged."
            });
        }
    }
}
