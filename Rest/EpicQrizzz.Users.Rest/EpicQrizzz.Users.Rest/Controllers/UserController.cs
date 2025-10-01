using EpicQrizzz.Users.Rest;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System;
using System.IO;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=Qrizz;User=lucas;Password=NegerBallen69!";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {


            var users = new List<User>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT username, munten FROM users";
                using var cmd = new MySqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var user = new User
                    {
                        Name = reader.GetString("username"),
                        Munten = reader.GetInt32("munten")
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

                    string sql = "SELECT username, uuid, munten FROM users WHERE uuid = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    user.Id = reader["uuid"].ToString();
                    user.Name = reader.GetString("username");
                    user.Munten = reader.GetInt32("munten");
                }
                return Ok(user);
            }
            catch
            {
                return NotFound();
            }

        }
        [HttpPost("Login/{username}")]
        public IActionResult Login([FromBody] Password enteredPassword, string username)
        {
            try
            {
                var storedPassword = "";
                Guid id = Guid.Empty;
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT password_hash, uuid FROM users WHERE username = @username";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@username", username);
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        return NotFound();
                    }

                    storedPassword = reader.GetString("password_hash");

                    id = reader.GetGuid("uuid");


                }
                if (enteredPassword.enteredPassword == storedPassword)
                {
                    return CreatedAtAction("Login", "user", new { Id = id, Name = username });
                }
                else
                {
                    return Unauthorized("Invalid password");
                }


            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPost("CreateAccount")]
        public IActionResult CreateAccount([FromBody] UserCreate user)
        {
            try
            {   
                if (user.Password.Length != 64) return NotFound();
                using var connection = new MySqlConnection(connectionString);
                {
                    connection.Open();
                    string sql = "INSERT INTO users (uuid, username, password_hash, munten) VALUES (@uuid, @username, @password_hash, 10); INSERT INTO inventory (userid, itemid, equipped) VALUES (@uuid, 1, 1)";
                    using var cmd = new MySqlCommand( sql, connection);
                    cmd.Parameters.AddWithValue("@uuid", user.Id);
                    cmd.Parameters.AddWithValue("@username", user.Name);
                    cmd.Parameters.AddWithValue("@password_hash", user.Password);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                }
                return CreatedAtAction("CreateAccount", "user", new { Id = user.Id, Name = user.Name, Munten = 10 });
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
                Console.WriteLine(ex.Message);
                return NotFound();

            }
        }
        [HttpPost("EditCoins/{id}")]
        public IActionResult EditCoins(string id, int prijs, string password)
        {
            try
            {
                string serverPassword = Environment.GetEnvironmentVariable("SERVERPASSWORD");
                if (password != serverPassword)
                {
                    return NotFound("You are not authorized");
                }
                using var connection = new MySqlConnection(connectionString);
                {
                    connection.Open();
                    string sql = @"UPDATE users
                           SET munten = munten + @prijs
                           WHERE uuid = @uuid";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@uuid", id);
                    cmd.Parameters.AddWithValue("@prijs", prijs);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return NotFound();
            }
            
        }
        [HttpPost("ChangeAvatar")]
        public IActionResult ChangeProfile()
        {
            return Ok();
        }
    }
}