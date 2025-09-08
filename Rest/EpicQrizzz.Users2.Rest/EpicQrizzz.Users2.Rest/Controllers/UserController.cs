using EpicQrizzz.Users2.Rest;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

using System.Reflection.PortableExecutable;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=EpicQrizzz;User=root;Password=";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            

            Console.WriteLine("checkpoint");
            var users = new List<User>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT uuid, username, password_hash FROM users";
                using var cmd = new MySqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = reader["uuid"].ToString(),
                        Name = reader.GetString("username")
                    };
                    users.Add(user);
                }
            }


            return Ok(users);

        }
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var user = new User();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT username, uuid FROM users WHERE uuid = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    user.Id = reader["uuid"].ToString();
                    user.Name = reader.GetString("username");
                }
                return Ok(user);
            }
            catch
            {
                return NotFound();
            }
            
        }

    }
}