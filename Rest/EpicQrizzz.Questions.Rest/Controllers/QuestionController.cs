using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Questions.Rest.Models;
using System.Reflection.PortableExecutable;
using Microsoft.AspNetCore.Authorization;



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
        public IActionResult GetById(int id)
        {
            try
            {
                var question = new Question();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM questions WHERE Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    question.Id = reader.GetInt32("Id");
                    question.QuestionText = reader.GetString("question_text");

                }

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM choices WHERE question_id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();
                    var awnsers = new List<Awnser>();
                    while (reader.Read())
                    {
                        var awnser = new Awnser();
                        awnser.Id = reader.GetInt32("Id");
                        awnser.AwnserText = reader.GetString("choice_text");
                        awnser.Option = reader.GetString("identifier");
                        awnsers.Add(awnser);
                    }

                    question.Options = awnsers;
                }
                return Ok(question);
            }
            catch (Exception ex)
            {
                // Log the exception or inspect it
                Console.WriteLine(ex.Message); // For debugging
                return StatusCode(500, "An error occurred."); // Better than NotFound
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