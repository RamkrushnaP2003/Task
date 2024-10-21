using ManagerNameSpace;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;  // Add for logging
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Manager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly string connectionString = "server=localhost;database=Task;user=ram;password=ram12345;";
        private readonly ILogger<ManagerController> _logger;

        public ManagerController(ILogger<ManagerController> logger)
        {
            _logger = logger;
        }

        // GET: api/manager/data
        [HttpGet("data")]
        public async Task<IActionResult> GetAllManagers()
        {
            try
            {
                List<ManagerModel> managers = new List<ManagerModel>();

                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand command = new MySqlCommand("GetAllManagers", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var manager = new ManagerModel
                                {
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Department = reader["Department"].ToString(),
                                    TeamSize = Convert.ToInt32(reader["TeamSize"])
                                };

                                managers.Add(manager);

                                _logger.LogInformation("Fetched Manager Data: {FirstName}, {LastName}, {Email}, {Department}, {TeamSize}",
                                    manager.FirstName, manager.LastName, manager.Email, manager.Department, manager.TeamSize);
                            }
                        }
                    }
                }

                if (managers.Count == 0)
                {
                    _logger.LogWarning("No managers found.");
                    return NotFound("No managers found");
                }

                return Ok(managers);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error fetching managers: {Message}", ex.Message);
                return BadRequest("Error: " + ex.Message);
            }
        }
    }
}
