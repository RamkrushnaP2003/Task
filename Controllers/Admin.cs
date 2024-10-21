using Employee;
using ManagerNameSpace;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserNamespace;

namespace Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly string connectionString = "server=localhost;database=Task;user=ram;password=ram12345;";

        private readonly ILogger<AdminController> _logger;

        public AdminController(ILogger<AdminController> logger) 
        {
            _logger = logger;
        }

        [HttpGet("data")]
        public async Task<IActionResult> ShowAllData() 
        {
            try 
            {
                List<EmployeeModel> employees = new List<EmployeeModel>();
                List<ManagerModel> managers = new List<ManagerModel>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Fetch Employees
                    using (MySqlCommand command = new MySqlCommand("GetAllEmployees", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure; 
                        
                        using (var reader = await command.ExecuteReaderAsync()) 
                        {
                            while (await reader.ReadAsync()) 
                            {
                                EmployeeModel employee = new EmployeeModel
                                {
                                    FirstName = reader["firstname"].ToString(),
                                    LastName = reader["lastname"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Department = reader["department"].ToString()
                                };

                                employees.Add(employee);
                                _logger.LogInformation("Fetched Employee Data: {FirstName}, {LastName}, {Email}, {Department}", employee.FirstName, employee.LastName, employee.Email, employee.Department);
                            }
                        }   
                    }

                    // Fetch Managers
                    using (MySqlCommand command = new MySqlCommand("GetAllManagers", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure; 
                        
                        using (var reader = await command.ExecuteReaderAsync()) 
                        {
                            while (await reader.ReadAsync()) 
                            {
                                ManagerModel manager = new ManagerModel
                                {
                                    FirstName = reader["firstname"].ToString(),
                                    LastName = reader["lastname"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Department = reader["department"].ToString(),
                                    TeamSize = Convert.ToInt32(reader["teamsize"])
                                };

                                managers.Add(manager);
                                _logger.LogInformation("Fetched Manager Data: {FirstName}, {LastName}, {Email}, {Department}, {TeamSize}", manager.FirstName, manager.LastName, manager.Email, manager.Department, manager.TeamSize);
                            }
                        }   
                    }
                }

                if(employees.Count == 0)
                {
                    _logger.LogWarning("No Employees Found");
                }

                if(managers.Count == 0)
                {
                    _logger.LogWarning("No Managers Found");
                }

                var result = new {
                    Employees = employees,
                    Managers = managers
                };

                return Ok(result);
            } 
            catch (MySqlException sqlEx)
            {
                _logger.LogError("Database Error: {Message}", sqlEx.Message);
                return BadRequest("Database error: " + sqlEx.Message);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Error fetching data: {Message}", ex.Message);
                return BadRequest("Error: " + ex.Message);
            }
        }

        [HttpPost("user")]
        public async Task<IActionResult> CreateUser([FromBody] UserModel request)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    if (request.UserType.ToLower() == "employee")
                    {
                        // Call stored procedure to add employee
                        using (MySqlCommand command = new MySqlCommand("AddEmployee", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@firstName", request.FirstName);
                            command.Parameters.AddWithValue("@lastName", request.LastName);
                            command.Parameters.AddWithValue("@email", request.Email);
                            command.Parameters.AddWithValue("@department", request.Department);

                            await command.ExecuteNonQueryAsync();
                        }
                        _logger.LogInformation("Employee created: {FirstName} {LastName}", request.FirstName, request.LastName);
                    }
                    else if (request.UserType.ToLower() == "manager")
                    {
                        // Call stored procedure to add manager
                        using (MySqlCommand command = new MySqlCommand("AddManager", connection))
                        {
                            command.CommandType = System.Data.CommandType.StoredProcedure;

                            command.Parameters.AddWithValue("@firstName", request.FirstName);
                            command.Parameters.AddWithValue("@lastName", request.LastName);
                            command.Parameters.AddWithValue("@email", request.Email);
                            command.Parameters.AddWithValue("@department", request.Department);
                            command.Parameters.AddWithValue("@teamSize", request.TeamSize);

                            await command.ExecuteNonQueryAsync();
                        }
                        _logger.LogInformation("Manager created: {FirstName} {LastName}", request.FirstName, request.LastName);
                    }
                    else
                    {
                        return BadRequest("Invalid User Type. Must be 'employee' or 'manager'.");
                    }
                }

                return CreatedAtAction(nameof(ShowAllData), new { message = "User created successfully." });
            }
            catch (MySqlException sqlEx)
            {
                _logger.LogError("Database Error: {Message}", sqlEx.Message);
                return BadRequest("Database error: " + sqlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error creating user: {Message}", ex.Message);
                return BadRequest("Error: " + ex.Message);
            }
        }
    }
}
