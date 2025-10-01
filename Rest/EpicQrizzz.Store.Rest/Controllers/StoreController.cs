using Microsoft.AspNetCore.Mvc;

using MySql.Data.MySqlClient;
using System.Text.Json;
namespace MyBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoreController : ControllerBase
    {
        string connectionString = "Server=localhost;Database=Qrizz;User=lucas;Password=NegerBallen69!";

        [HttpPost("BuyPack/{packid}/{userid}")]
        public async Task<IActionResult> BuyPack(int packid, string userid)
        {
            try
            {
                string serverPassword = Environment.GetEnvironmentVariable("SERVERPASSWORD");

                // Random item generator (kept inline)
                static int randomItemIdGenerator(int itemId1, int itemId2)
                {
                    Random rnd = new Random();
                    return rnd.Next(itemId1, itemId2);
                }

                int randomItemId = packid switch
                {
                    1 => randomItemIdGenerator(2, 6),
                    2 => randomItemIdGenerator(7, 11),
                    3 => randomItemIdGenerator(12, 16),
                    _ => 0
                };

                // Call EditCoins API asynchronously
                using var httpClient = new HttpClient();
                string url = $"http://joost.assenbergh.nl:5292/api/user/GetById/{userid}";
                var res = await httpClient.GetAsync(url);
                res.EnsureSuccessStatusCode();
                string json = await res.Content.ReadAsStringAsync();
                if (JsonDocument.Parse(json).RootElement.GetProperty("munten").GetInt32() >= 5)
                {
                    using var httpClientForEditCoins = new HttpClient();
                    string url2 = $"http://joost.assenbergh.nl:5292/api/user/EditCoins/{userid}?prijs=-5&password={serverPassword}";
                    var response = await httpClient.PostAsync(url2, null);
                    response.EnsureSuccessStatusCode();
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(res);
                        return StatusCode((int)res.StatusCode, "Failed to deduct coins");
                    }
                }
                else 
                {
                    Console.WriteLine("Je hebt niet genoeg muntjes!");
                    return StatusCode(601);
                }

                

                // Insert item into inventory
                await using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                string sql = "INSERT INTO inventory (userid, itemid, equipped) VALUES (@userid, @itemid, 0)";
                await using var cmd = new MySqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@userid", userid);
                cmd.Parameters.AddWithValue("@itemid", randomItemId);

                int rows = await cmd.ExecuteNonQueryAsync();

                if (rows == 0)
                {
                    return StatusCode(500, "Failed to insert item");
                }

                return CreatedAtAction("BuyPack", "item", new { itemId = randomItemId });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(500, "Something went wrong");
            }
        }
    }
}