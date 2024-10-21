using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using UserNamespace;

namespace User.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly string connectionString = "server=localhost;database=Task;user=ram;password=ram12345;";
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger) 
        {
            _logger = logger;
        }

        // GET: api/user/{email}
        [HttpGet("/login/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email) 
        {
            try
            {
                UserModel user = null;

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Check in the employees table
                    string employeeQuery = "SELECT firstname, lastname, email, department, 'employee' AS userType, NULL AS teamSize FROM employees WHERE email = @Email";
                    
                    using (MySqlCommand command = new MySqlCommand(employeeQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Email", email);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync()) 
                            {
                                user = new UserModel
                                {
                                    FirstName = reader["firstname"].ToString(),
                                    LastName = reader["lastname"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Department = reader["department"].ToString(),
                                    UserType = "employee", // Set the UserType explicitly
                                    TeamSize = null // Employees do not have a team size
                                };
                                // Log employee details
                                _logger.LogInformation("Fetched Employee: {FirstName} {LastName}, Email: {Email}, Department: {Department}", user.FirstName, user.LastName, user.Email, user.Department);
                            }
                        }
                    }

                    // If not found in employees, check in the managers table
                    if (user == null)
                    {
                        string managerQuery = "SELECT firstname, lastname, email, department, 'manager' AS userType, teamSize FROM managers WHERE email = @Email";
                        
                        using (MySqlCommand command = new MySqlCommand(managerQuery, connection))
                        {
                            command.Parameters.AddWithValue("@Email", email);

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync()) 
                                {
                                    user = new UserModel
                                    {
                                        FirstName = reader["firstname"].ToString(),
                                        LastName = reader["lastname"].ToString(),
                                        Email = reader["email"].ToString(),
                                        Department = reader["department"].ToString(),
                                        UserType = "manager", // Set the UserType explicitly
                                        TeamSize = reader.IsDBNull(reader.GetOrdinal("teamSize")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("teamSize"))
                                    };
                                    // Log manager details
                                    _logger.LogInformation("Fetched Manager: {FirstName} {LastName}, Email: {Email}, Department: {Department}, Team Size: {TeamSize}", user.FirstName, user.LastName, user.Email, user.Department, user.TeamSize);
                                }
                            }
                        }
                    }
                }

                if (user == null)
                {
                    return NotFound(new { message = "User not found." });
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching user: {Message}", ex.Message);
                return BadRequest(new { message = "Error: " + ex.Message });
            }
        }
    }
}
