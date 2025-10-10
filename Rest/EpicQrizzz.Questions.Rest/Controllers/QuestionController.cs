using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Questions.Rest.Models;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authorization;
using System.Data;



namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuetionController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=Qrizz;User=lucas;Password=NegerBallen69!";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                var questions = new List<Question>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM questions";
                    using var cmd = new MySqlCommand(sql, connection);
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var question = new Question
                        {
                            Id = reader.GetInt32("Id"),
                            QuestionText = reader.GetString("question_text")
                        };

                        questions.Add(question);
                    }
                }

                // Fetch choices for each question
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (var question in questions)
                    {
                        string sql = "SELECT * FROM choices WHERE question_id = @id";
                        using var cmd = new MySqlCommand(sql, connection);
                        cmd.Parameters.AddWithValue("@id", question.Id);

                        using var reader = cmd.ExecuteReader();
                        var answers = new List<Awnser>();

                        while (reader.Read())
                        {
                            var answer = new Awnser
                            {
                                Id = reader.GetInt32("Id"),
                                AwnserText = reader.GetString("choice_text"),
                                Option = reader.GetString("identifier")
                            };

                            answers.Add(answer);
                        }

                        question.Options = answers;
                    }
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }



        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var question = new Question();

                // First DB query: get the question
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string sql = "SELECT * FROM questions WHERE Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        question.Id = reader.GetInt32("Id");
                        question.QuestionText = reader.GetString("question_text");
                    }
                    else
                    {
                        return NotFound($"Question with Id={id} not found.");
                    }
                }

                // Second DB query: get the choices
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string sql = "SELECT * FROM choices WHERE question_id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    using var reader = await cmd.ExecuteReaderAsync();
                    var answers = new List<Awnser>();
                    while (await reader.ReadAsync())
                    {
                        var answer = new Awnser
                        {
                            Id = reader.GetInt32("Id"),
                            AwnserText = reader.GetString("choice_text"),
                            Option = reader.GetString("identifier")
                        };
                        answers.Add(answer);
                    }

                    question.Options = answers;
                }

                return Ok(question);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message); // For debugging
                return StatusCode(500, "An error occurred.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Add")]
        public IActionResult Add([FromBody] Question question)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert question
                    string sql = "INSERT INTO questions (question_text) VALUES (@text); SELECT LAST_INSERT_ID();";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@text", question.QuestionText);

                    var newId = Convert.ToInt32(cmd.ExecuteScalar());
                    question.Id = newId;

                    // Insert choices if provided
                    if (question.Options != null && question.Options.Any())
                    {
                        foreach (var option in question.Options)
                        {
                            string choiceSql = "INSERT INTO choices (choice_text, identifier, question_id) VALUES (@text, @identifier, @qid)";
                            using var choiceCmd = new MySqlCommand(choiceSql, connection);
                            choiceCmd.Parameters.AddWithValue("@text", option.AwnserText);
                            choiceCmd.Parameters.AddWithValue("@identifier", option.Option);
                            choiceCmd.Parameters.AddWithValue("@qid", newId);
                            choiceCmd.ExecuteNonQuery();
                        }
                    }
                }

                return Ok(question); // Return the created question with Id
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }



        [HttpGet("CheckAnswer/{id}/{choice}")]
        public IActionResult CheckAnswer(int id, string choice)
        {
            try
            {
                bool correct = false;

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"SELECT is_correct 
                           FROM choices 
                           WHERE question_id = @id AND identifier = @choice";

                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@choice", choice.ToUpper());

                    var result = cmd.ExecuteScalar();

                    if (result != null && Convert.ToBoolean(result))
                    {
                        correct = true;
                    }
                }

                return Ok(correct);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "An error occurred.");
            }
        }

    }
}