using Employee; // If the class is renamed, this needs to be updated accordingly
using Microsoft.AspNetCore.Mvc; 
using MySql.Data.MySqlClient;

namespace Employee.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase 
    {
        private readonly string connectionString = "server=localhost;database=Task;user=ram;password=ram12345";

        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(ILogger<EmployeeController> logger)
        {
            _logger = logger;
        }

        // GET: api/employee/data
        [HttpGet("data")]
        public async Task<IActionResult> GetAllEmployees()
        {
            try 
            {
                List<EmployeeModel> employees = new List<EmployeeModel>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("GetAllEmployees", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure; 
                        
                        using (var reader = await command.ExecuteReaderAsync()) {
                            while (await reader.ReadAsync()) 
                            {
                                EmployeeModel employee = new EmployeeModel
                                {
                                    FirstName = reader["firstName"].ToString(),
                                    LastName = reader["lastName"].ToString(),
                                    Email = reader["email"].ToString(),
                                    Department = reader["department"].ToString()
                                };

                                employees.Add(employee);

                                _logger.LogInformation("Fetched Employee Data : {FirstName}, {LastName}, {Email}, {Department}", employee.FirstName, employee.LastName, employee.Email, employee.Department);
                            }
                        }   
                    }
                }

                if(employees.Count == 0)
                {
                    _logger.LogWarning("No Employee Found");
                    return NotFound("No Employee found");
                }

                return Ok(employees);
            } 
            catch (MySqlException sqlEx)
            {
                _logger.LogError("DataBase Err : " + sqlEx.Message);
                return BadRequest("Database error: " + sqlEx.Message);
            }
            catch (Exception ex) 
            {
                _logger.LogError("Error fetching Employees: {Message}", ex.Message);
                return BadRequest("Error: " + ex.Message);
            }
        }
    }
}
