using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Game.Rest.Models;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;



namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=Qrizz;User=lucas;Password=NegerBallen69!";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var games = new List<GameModel>();

                // Fetch choices for each question
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    
                        string sql = "SELECT room, user_id, score FROM game";
                        using var cmd = new MySqlCommand(sql, connection);
                        using var reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            var game = new GameModel
                            {
                                Room = reader.GetString("room"),
                                UserId = reader.GetGuid("user_id").ToString(),
                                Score = reader.GetInt32("score").ToString()
                            };
                        games.Add(game);

                    }
                }
                return Ok(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpGet("GetRoom/{room}")]
        public async Task<IActionResult> GetRoom(string room)
        {
            try
            {
                var games = new List<GameModel>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT room, user_id, score FROM game WHERE room = @room";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@room", room);

                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var userId = reader.GetGuid("user_id").ToString();

                        // Fetch username from your API
                        string username = await GetUsernameById(userId);

                        var game = new GameModel
                        {
                            Room = reader.GetString("room"),
                            UserId = username, // now we use username instead of id
                            Score = reader.GetInt32("score").ToString()
                        };
                        games.Add(game);
                    }
                }

                if (games.Count == 0)
                {
                    return NotFound($"No games found for room '{room}'");
                }

                return Ok(games);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }

        // Helper method to call the API and get username
        private async Task<string> GetUsernameById(string userId)
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"http://joost.assenbergh.nl:5292/api/user/GetById/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserModel>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                return user?.Name ?? "Unknown";
            }
            return "Unknown";
        }

        // Model for the user API response
        public class UserModel
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }



        [HttpGet("GetById/{user_id}")]
        public IActionResult GetById(Guid user_id)
        {
            try
            {
                var game = new GameModel();

                // Fetch choices for each question
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM game WHERE user_id = @user_id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@user_id", user_id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    game.Room = reader.GetString("room");
                    game.UserId = reader.GetGuid("user_id").ToString();
                    game.Score = reader.GetInt32("score").ToString();


                }
                return Ok(game);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }



        [HttpPost("Add")]
        public IActionResult Add([FromBody] GameModel game)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"INSERT INTO game (room, user_id, score) 
                           VALUES (@room, @user_id, @score)";

                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@room", game.Room);
                    cmd.Parameters.AddWithValue("@user_id", game.UserId);
                    cmd.Parameters.AddWithValue("@score", game.Score);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                        return Ok("Game added successfully.");
                    else
                        return BadRequest("Failed to add game.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }



        [HttpPost("JoinOrAdd")]
        public IActionResult JoinOrAdd([FromBody] GameModel game)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Delete any previous game for this user
                    string deleteSql = "DELETE FROM game WHERE user_id = @user_id";
                    using (var deleteCmd = new MySqlCommand(deleteSql, connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@user_id", game.UserId);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Insert the new game (score always starts at 0)
                    string insertSql = @"INSERT INTO game (room, user_id, score) 
                                 VALUES (@room, @user_id, 0)";
                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@room", game.Room);
                        insertCmd.Parameters.AddWithValue("@user_id", game.UserId);

                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            return Ok("Joined or created game successfully with score 0.");
                        else
                            return BadRequest("Failed to join or create game.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }


        [HttpGet("CheckGameAnswer/{userId}/{questionId}/{answer}")]
        public async Task<IActionResult> CheckGameAnswer(string userId, int questionId, string answer)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Call external API
                    string url = $"http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/{questionId}/{answer}";
                    var response = await httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                        return StatusCode((int)response.StatusCode, "External API call failed.");

                    var result = await response.Content.ReadAsStringAsync();

                    if (!bool.TryParse(result, out bool isCorrect))
                        return BadRequest("Invalid response from external API.");

                    if (isCorrect)
                    {
                        using (var connection = new MySqlConnection(connectionString))
                        {
                            connection.Open();

                            // Increase score by 1 for this user
                            string sql = @"UPDATE game 
                                   SET score = score + 1 
                                   WHERE user_id = @user_id";

                            using var cmd = new MySqlCommand(sql, connection);
                            cmd.Parameters.AddWithValue("@user_id", userId);

                            int rowsAffected = cmd.ExecuteNonQuery();

                            if (rowsAffected == 0)
                                return NotFound("User not found in game table.");
                        }
                    }

                    return Ok(new { userId, questionId, answer, isCorrect });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Error calling external API or updating DB.");
            }
        }





    }
}