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
        string connectionString = "Server=localhost;Database=Qrizz;User=lucas;Password=NegerBallen69!";

        [HttpGet("GetAll")]
        public IActionResult GetAll()
        {
            var inventories = new List<Inventory>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT itemid, equipped FROM inventory";
                using var cmd = new MySqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var inventory = new Inventory
                    {
                        itemId = reader.GetInt32("itemid"),
                        equipped = reader.GetInt32("equipped")
                    };
                    inventories.Add(inventory);
                }
            }
            return Ok(inventories);
        }
        [HttpGet("CheckEquipped/{id}")]
        public IActionResult CheckEquipped(string id)
        {
            try
            {
                var inventory = new Inventory();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT itemid, equipped FROM inventory WHERE userid = @uuid AND equipped = 1 ";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@uuid", id);
                    using var reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        return NotFound();
                    }

                    inventory.itemId = reader.GetInt32("itemid");
                    inventory.equipped = reader.GetInt32("equipped");
                }
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }
        [HttpPost("Equip/{itemid}/{userid}")]
        public IActionResult Equip(int itemid, string userid)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "UPDATE inventory SET equipped = 0 WHERE userid = @userid; UPDATE inventory SET equipped = 1 WHERE userid = @userid AND itemid = @itemid";
                    using var cmd = new MySqlCommand( sql, connection);
                    cmd.Parameters.AddWithValue("userid", userid);
                    cmd.Parameters.AddWithValue("itemid", itemid);
                    using var reader = cmd.ExecuteReader();

                    reader.Read();

                    return CreatedAtAction("Equip", "itemid", new { itemId = itemid } );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }
        [HttpGet("GetById/{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var inventories = new List<Inventory>();

                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT itemid, equipped FROM inventory WHERE userid = @uuid";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@uuid", id);
                    using var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var inventory = new Inventory
                        {
                            itemId = reader.GetInt32("itemid"),
                            equipped = reader.GetInt32("equipped")
                        };
                        inventories.Add(inventory);
                    }
                }
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound();
            }
        }
    }
}
    