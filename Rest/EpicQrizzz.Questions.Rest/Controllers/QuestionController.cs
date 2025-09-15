using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using EpicQrizzz.Quetions.Rest.Models;
using System.Reflection.PortableExecutable;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuetionController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=kennisquiz;User=root;Password=";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            try
            {
                Console.WriteLine("checkpoint");
                var questions = new List<Question>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT Id, Question, OptionA, OptionB, OptionC, OptionD, CorrectOption FROM Questions";
                    using var cmd = new MySqlCommand(sql, connection);
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var question = new Question
                        {
                            Id = reader.GetInt32("Id"),
                            QuestionText = reader.GetString("Question"),
                            OptionA = reader.GetString("OptionA"),
                            OptionB = reader.GetString("OptionB"),
                            OptionC = reader.GetString("OptionC"),
                            OptionD = reader.GetString("OptionD"),
                        };
                        questions.Add(question);
                    }
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                // Return a simple error message
                Console.WriteLine(ex.Message);

                return BadRequest(new { error = ex.Message });
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

                    string sql = "SELECT * FROM Questions WHERE Id = @id";
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

                    string identifier = "";
                    bool is_correct = false;

                    while (reader.Read())
                    {
                        identifier = reader.GetString("identifier");
                        is_correct = reader.GetBoolean("is_correct");
                        if (identifier == "A1") { question.OptionA = reader.GetString("choice_text"); }
                        else if (identifier == "A2") { question.OptionB = reader.GetString("choice_text"); }
                        else if (identifier == "A3") { question.OptionC = reader.GetString("choice_text"); }
                        else if (identifier == "A4") { question.OptionD = reader.GetString("choice_text"); }

                        if (is_correct){ question.CorrectOption = reader.GetString("identifier"); }
                    }
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

        [HttpPost("Add")]
        public IActionResult Add([FromBody] Question question)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"INSERT INTO Questions 
                          (Question, OptionA, OptionB, OptionC, OptionD, CorrectOption) 
                          VALUES (@Question, @OptionA, @OptionB, @OptionC, @OptionD, @CorrectOption);
                          SELECT LAST_INSERT_ID();";

                    using var cmd = new MySqlCommand(sql, connection);

                    cmd.Parameters.AddWithValue("@Question", question.QuestionText);
                    cmd.Parameters.AddWithValue("@OptionA", question.OptionA);
                    cmd.Parameters.AddWithValue("@OptionB", question.OptionB);
                    cmd.Parameters.AddWithValue("@OptionC", question.OptionC);
                    cmd.Parameters.AddWithValue("@OptionD", question.OptionD);
                    cmd.Parameters.AddWithValue("@CorrectOption", question.CorrectOption); // <-- added

                    // Execute and get new ID
                    var newId = Convert.ToInt32(cmd.ExecuteScalar());
                    question.Id = newId;
                }

                return CreatedAtAction(nameof(GetById), new { id = question.Id }, question);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpGet("GetAwnserById/{id}")]
        public IActionResult GetAwnserById(int id)
        {
            try
            {
                var awnser = new FullAwnser();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Questions WHERE Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    awnser.CorrectOption = reader.GetChar("CorrectOption");

                }
                return Ok(awnser);
            }
            catch
            {
                return NotFound();

            }
        }


        [HttpGet("CheckAwnser/{id}/{choice}")]
        public IActionResult CheckAwnser(int id, char choice)
        {
            try
            {
                var awnser = new FullAwnser();
                bool correct = false;
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM Questions WHERE Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    awnser.CorrectOption = reader.GetChar("CorrectOption");
                    if (awnser.CorrectOption.ToString() == choice.ToString().ToUpper())
                    {
                        correct = true;
                    }

                }
                return Ok(correct);
            }
            catch
            {
                return NotFound();

            }
        }
    }
}