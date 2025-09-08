using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Reflection.PortableExecutable;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=EpicQrizzz;User=lucas;Password=NegerBallen69!";


        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {


            return Ok();

        }


        
    }
}