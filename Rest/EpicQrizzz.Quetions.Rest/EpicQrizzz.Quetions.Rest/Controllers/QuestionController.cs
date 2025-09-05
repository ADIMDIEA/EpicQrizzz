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
        string connectionString = "Host=localhost;Port=3306;Database=EpicQrizzz;User=root;Password=;";

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
            catch
            {
                return NotFound();

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
                    question.QuestionText = reader.GetString("Question");
                    question.OptionA = reader.GetString("OptionA");
                    question.OptionB = reader.GetString("OptionB");
                    question.OptionC = reader.GetString("OptionC");
                    question.OptionD = reader.GetString("OptionD");
                }
                return Ok(question);
            }
            catch
            {
                return NotFound();

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