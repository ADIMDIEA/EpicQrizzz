using EpicQrizzz.Users2.Rest;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using System.Security.Cryptography;
using System.Reflection.PortableExecutable;
using System.Text;
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


            var users = new List<User>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT uuid, username FROM users";
                using var cmd = new MySqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var user = new User
                    {
                        Id = reader["uuid"].ToString(),
                        Name = reader.GetString("username"),
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
        [HttpPost("Login/{id}")]
        public IActionResult Login([FromBody] Password enteredPassword, string id)
        {
            try
            {
                var password = new Password();
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT password_hash, username FROM users WHERE uuid = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    password.enteredPassword = reader.GetString("password_hash");


                }
                if (enteredPassword.enteredPassword == password.enteredPassword)
                {
                    return Ok();
                }
                else
                {
                    return NotFound();
                }


            }
            catch
            {
                return NotFound();
            }
        }
        [HttpPost("CreateAccount")]
        public IActionResult CreateAccount([FromBody] UserCreate user)
        {
            try
            {   
                if (user.Password.Length != 64) return Forbid();
                using var connection = new MySqlConnection(connectionString);
                {
                    connection.Open();
                    string sql = "INSERT INTO users (uuid, username, password_hash) VALUES (@uuid, @username, @password_hash)";
                    using var cmd = new MySqlCommand( sql, connection);
                    cmd.Parameters.AddWithValue("@uuid", user.Id);
                    cmd.Parameters.AddWithValue("@username", user.Name);
                    cmd.Parameters.AddWithValue("@password_hash", user.Password);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                }
                return CreatedAtAction("CreateAccount", "user", new { Id = user.Id, Name = user.Name, Password = user.Password });
                    
            }
            catch
            {
                return Forbid();
            }
        }
    }
}