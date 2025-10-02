using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Game.Rest.Models;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Diagnostics;



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

                    // Check if the room already exists
                    string checkRoomSql = "SELECT COUNT(*) FROM game WHERE room = @room";
                    bool isHost = false;
                    using (var checkCmd = new MySqlCommand(checkRoomSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@room", game.Room);
                        int roomCount = Convert.ToInt32(checkCmd.ExecuteScalar());
                        isHost = roomCount == 0; // First player in room = host
                    }

                    // If it's a new room -> generate 10 random questions
                    if (isHost)
                    {
                        Random rnd = new Random();
                        var questions = Enumerable.Range(1, 150)
                                                  .OrderBy(x => rnd.Next())
                                                  .Take(10)
                                                  .ToArray();

                        string insertRoomSql = @"INSERT INTO room_questions 
                                         (room, question_number, question_id) 
                                         VALUES (@room, @qnum, @qid)";

                        for (int i = 0; i < questions.Length; i++)
                        {
                            using (var insertRoomCmd = new MySqlCommand(insertRoomSql, connection))
                            {
                                insertRoomCmd.Parameters.AddWithValue("@room", game.Room);
                                insertRoomCmd.Parameters.AddWithValue("@qnum", i + 1); // 1..10
                                insertRoomCmd.Parameters.AddWithValue("@qid", questions[i]);
                                insertRoomCmd.ExecuteNonQuery();
                            }
                        }
                    }

                    // Insert the player into the game table
                    string insertSql = @"INSERT INTO game (room, user_id, score, host, start, question) 
                                 VALUES (@room, @user_id, 0, @host, 0, 0)";
                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        insertCmd.Parameters.AddWithValue("@room", game.Room);
                        insertCmd.Parameters.AddWithValue("@user_id", game.UserId);
                        insertCmd.Parameters.AddWithValue("@host", isHost ? 1 : 0);

                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                            return Ok(new { message = "Joined or created game successfully with score 0.", host = isHost });
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
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if the user exists and if their room has started
                    string checkSql = @"SELECT g.start, g.question 
                                FROM game g 
                                WHERE g.user_id = @user_id";
                    bool roomStarted = false;
                    int currentQuestionIndex = 0;

                    using (var checkCmd = new MySqlCommand(checkSql, connection))
                    {
                        checkCmd.Parameters.AddWithValue("@user_id", userId);
                        using (var reader = checkCmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                roomStarted = Convert.ToBoolean(reader["start"]);
                                currentQuestionIndex = Convert.ToInt32(reader["question"]);
                            }
                            else
                            {
                                return NotFound("User not found in game table.");
                            }
                        }
                    }

                    if (!roomStarted)
                    {
                        return BadRequest("The game has not started yet for this room.");
                    }

                    // Call external API to check answer
                    using (var httpClient = new HttpClient())
                    {
                        string url = $"http://joost.assenbergh.nl:5291/api/quetion/CheckAnswer/{questionId}/{answer}";
                        var response = await httpClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                            return StatusCode((int)response.StatusCode, "External API call failed.");

                        var result = await response.Content.ReadAsStringAsync();

                        if (!bool.TryParse(result, out bool isCorrect))
                            return BadRequest("Invalid response from external API.");

                        // Increment question index regardless of correctness
                        string updateQuestionSql = @"UPDATE game SET question = question + 1 WHERE user_id = @user_id";
                        using (var updateCmd = new MySqlCommand(updateQuestionSql, connection))
                        {
                            updateCmd.Parameters.AddWithValue("@user_id", userId);
                            updateCmd.ExecuteNonQuery();
                        }

                        // Increase score if correct
                        if (isCorrect)
                        {
                            string updateScoreSql = @"UPDATE game SET score = score + 1 WHERE user_id = @user_id";
                            using (var updateScoreCmd = new MySqlCommand(updateScoreSql, connection))
                            {
                                updateScoreCmd.Parameters.AddWithValue("@user_id", userId);
                                updateScoreCmd.ExecuteNonQuery();
                            }
                        }

                        return Ok(new
                        {
                            userId,
                            questionId,
                            answer,
                            isCorrect,
                            nextQuestionIndex = currentQuestionIndex + 1
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Error calling external API or updating DB.");
            }
        }


        [HttpPost("StartGame")]
        public IActionResult StartGame([FromBody] Guid userId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Get the room and check if the user is the host
                    string checkHostSql = "SELECT room, host FROM game WHERE user_id = @user_id";
                    string room = null;
                    bool isHost = false;

                    using (var cmd = new MySqlCommand(checkHostSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                room = reader["room"].ToString();
                                isHost = Convert.ToBoolean(reader["host"]);
                            }
                            else
                            {
                                return BadRequest("User not found in any game.");
                            }
                        }
                    }

                    if (!isHost)
                    {
                        return BadRequest("Only the host can start the game.");
                    }

                    // 2. Set start = true for all players in the same room
                    string updateStartSql = "UPDATE game SET start = 1 WHERE room = @room";
                    using (var updateCmd = new MySqlCommand(updateStartSql, connection))
                    {
                        updateCmd.Parameters.AddWithValue("@room", room);
                        int rowsAffected = updateCmd.ExecuteNonQuery();

                        return Ok(new { message = $"Game started for {rowsAffected} player(s) in room {room}." });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }

        [HttpGet("IsGameStarted/{userId}")]
        public IActionResult IsGameStarted(string userId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT start FROM game WHERE user_id = @user_id";
                    using (var cmd = new MySqlCommand(sql, connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                            return NotFound("User not found in game table.");

                        bool started = Convert.ToBoolean(result);
                        return Ok(new { userId, started });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while checking game start status.");
            }
        }



        [HttpGet("GetGameQuestion/{userId}")]
        public async Task<IActionResult> GetGameQuestion(string userId)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    Console.WriteLine($"[LOG] Opened DB connection in {stopwatch.ElapsedMilliseconds} ms");

                    // 1. Check if the user is in a started room
                    stopwatch.Restart();
                    string checkRoomSql = "SELECT room, question, start FROM game WHERE user_id = @user_id";
                    string room = null;
                    int questionIndex = 0;
                    bool roomStarted = false;

                    using (var cmd = new MySqlCommand(checkRoomSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                room = reader["room"].ToString();
                                questionIndex = Convert.ToInt32(reader["question"]);
                                roomStarted = Convert.ToBoolean(reader["start"]);
                            }
                            else
                            {
                                return NotFound("User not found in game table.");
                            }
                        }
                    }
                    Console.WriteLine($"[LOG] Fetched user game info in {stopwatch.ElapsedMilliseconds} ms");

                    if (!roomStarted)
                        return BadRequest("The game has not started yet for this room.");

                    // 2. Get the question_id from room_questions for this room and index
                    stopwatch.Restart();
                    string questionSql = @"SELECT question_id 
                                       FROM room_questions 
                                       WHERE room = @room AND question_number = @qnum";
                    int? questionId = null;

                    using (var cmd = new MySqlCommand(questionSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@room", room);
                        cmd.Parameters.AddWithValue("@qnum", questionIndex + 1); // question_number starts at 1
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            questionId = Convert.ToInt32(result);
                        }
                        else
                        {
                            return BadRequest("No more questions in this room.");
                        }
                    }
                    Console.WriteLine($"[LOG] Retrieved question_id in {stopwatch.ElapsedMilliseconds} ms");

                    // 3. Call external API to get question by id
                    stopwatch.Restart();
                    using (var httpClient = new HttpClient())
                    {
                        string url = $"http://joost.assenbergh.nl:5291/api/quetion/GetById/{questionId}";
                        var response = await httpClient.GetAsync(url);

                        if (!response.IsSuccessStatusCode)
                            return StatusCode((int)response.StatusCode, "External API call failed.");

                        var questionData = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"[LOG] External API call took {stopwatch.ElapsedMilliseconds} ms");

                        // 4. Return the external API JSON directly WITHOUT incrementing
                        return Content(questionData, "application/json");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the question.");
            }
        }



    [HttpGet("EndGame/{userId}")]
        public async Task<IActionResult> EndGame(string userId)
        {
            try
            {
                using var connection = new MySqlConnection(connectionString);
                connection.Open();

                // 1. Get the user's room
                string getRoomSql = "SELECT room FROM game WHERE user_id = @user_id";
                string room = null;
                using (var cmd = new MySqlCommand(getRoomSql, connection))
                {
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    room = cmd.ExecuteScalar()?.ToString();
                }

                if (room == null)
                    return NotFound("User not found in any game.");

                // 2. Check if everyone in the room has reached question 10
                string checkFinishedSql = "SELECT COUNT(*) FROM game WHERE room = @room AND question < 10";
                int unfinishedCount = 0;
                using (var cmd = new MySqlCommand(checkFinishedSql, connection))
                {
                    cmd.Parameters.AddWithValue("@room", room);
                    unfinishedCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (unfinishedCount > 0)
                {
                    return Ok(new { message = "Not everyone has finished the game yet." });
                }

                // 3. Get the top 3 players by score
                string topPlayersSql = @"SELECT user_id, score FROM game 
                                 WHERE room = @room 
                                 ORDER BY score DESC 
                                 LIMIT 3";
                var topPlayers = new List<(string userId, int score)>();
                using (var cmd = new MySqlCommand(topPlayersSql, connection))
                {
                    cmd.Parameters.AddWithValue("@room", room);
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        topPlayers.Add((reader["user_id"].ToString(), Convert.ToInt32(reader["score"])));
                    }
                }

                if (topPlayers.Count == 0)
                    return BadRequest("No players found in this room.");

                string topWinnerId = topPlayers[0].userId;

                // 4. Give coins and delete room/questions if the caller is the top winner
                if (userId == topWinnerId)
                {
                    try
                    {
                        using var httpClientForEditCoins = new HttpClient();
                        string serverPassword = Environment.GetEnvironmentVariable("SERVERPASSWORD");
                        if (string.IsNullOrEmpty(serverPassword))
                            return StatusCode(500, "Server password not set in environment variables.");

                        string url = $"http://joost.assenbergh.nl:5292/api/user/EditCoins/{topWinnerId}?prijs=10&password={serverPassword}";
                        var response = await httpClientForEditCoins.PostAsync(url, null);
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to give coins: " + ex.Message);
                    }

                    // Delete the room and all associated questions
                    string deleteRoomSql = "DELETE FROM game WHERE room = @room";
                    using (var cmd = new MySqlCommand(deleteRoomSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@room", room);
                        cmd.ExecuteNonQuery();
                    }

                    string deleteQuestionsSql = "DELETE FROM room_questions WHERE room = @room";
                    using (var cmd = new MySqlCommand(deleteQuestionsSql, connection))
                    {
                        cmd.Parameters.AddWithValue("@room", room);
                        cmd.ExecuteNonQuery();
                    }
                }

                // 5. Replace UUIDs with usernames for top 3
                var top3WithNames = new List<object>();
                foreach (var player in topPlayers)
                {
                    string name = await GetUsernameById(player.userId); // Use your existing function
                    top3WithNames.Add(new { name, score = player.score });
                }

                return Ok(new
                {
                    top3 = top3WithNames,
                    coinsGivenToTopWinner = userId == topWinnerId
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred while ending the game.");
            }
        }


    }
}