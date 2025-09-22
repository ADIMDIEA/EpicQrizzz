using EpicQrizzz.Inventory.Rest;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Org.BouncyCastle.Bcpg;
using System.Globalization;

namespace MyBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InventoryController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=epicqrizzz;User=root;Password=";

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var inventories = new List<Inventory>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT userid, itemid FROM inventory";
                using var cmd = new MySqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var inventory = new Inventory
                    {
                        userId = reader["userid"].ToString(),
                        itemId = reader.GetInt32("itemid"),
                    };
                    inventories.Add(inventory);
                }
            }
            return Ok(inventories);
        }

    }
}
    