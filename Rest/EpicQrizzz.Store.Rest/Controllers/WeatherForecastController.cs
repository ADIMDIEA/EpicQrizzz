using Microsoft.AspNetCore.Mvc;

using MySql.Data.MySqlClient;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=EpicQrizzz;User=root;Password=";

        [HttpGet("GetAll")]
        public ActionResult GetAll() 
        {

            return Ok();
        }
    }
}