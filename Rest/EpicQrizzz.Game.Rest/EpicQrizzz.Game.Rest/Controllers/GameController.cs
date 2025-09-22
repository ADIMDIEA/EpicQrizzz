using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Game.Rest.Models;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authorization;



namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=kennisquiz;User=root;Password=";


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
                                Id = reader.GetGuid("user_id").ToString(),
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

        [HttpGet("GetById/{id}")]
        public IActionResult GetById(int id)
        {
            try
            {
                var game = new GameModel();

                // Fetch choices for each question
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM game WHERE Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    game.Room = reader.GetString("room");
                    game.Id = reader.GetString("user_id");
                    game.Score = reader.GetString("score");
                    
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
                    cmd.Parameters.AddWithValue("@user_id", game.Id);
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
                        deleteCmd.Parameters.AddWithValue("@user_id", game.Id);
                        deleteCmd.ExecuteNonQuery();
                    }

                    // Insert the new game
                    string insertSql = @"INSERT INTO game (room, user_id, score) 
                                 VALUES (@room, @user_id, @score)";
                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@room", game.Room);
                        insertCmd.Parameters.AddWithValue("@user_id", game.Id);
                        insertCmd.Parameters.AddWithValue("@score", game.Score);

                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            return Ok("Joined or created game successfully.");
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

        [HttpGet("CheckGameAnswer/{questionId}/{answer}")]
        public async Task<IActionResult> CheckGameAnswer(int questionId, string answer)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = $"http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/{questionId}/{answer}";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        // Expecting true/false from external API
                        if (bool.TryParse(result, out bool isCorrect))
                        {
                            return Ok(new { questionId, answer, isCorrect });
                        }
                        else
                        {
                            return BadRequest("Invalid response from external API.");
                        }
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, "External API call failed.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Error calling external API.");
            }
        }




    }
}