using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace BlazorApp.Api.Golfer
{
    public static class GolferFunction
    {
        [FunctionName("CreateGolfer")]
        public static async Task<IActionResult> CreateGolfer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "golfer")] HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<Shared.Models.Golfer>(requestBody);
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    if (!string.IsNullOrEmpty(input.FirstName))
                    {
                        var dateNow = DateTime.UtcNow;
                        var query = $"INSERT INTO [Golfer] (FirstName,LastName,Email,Handicap, CreatedOn) VALUES('{input.FirstName}', '{input.LastName}', '{input.Email}', '{input.Handicap}','{dateNow}')";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
                return new BadRequestResult();
            }
            return new OkResult();
        }

        [FunctionName("UpdateGolfer")]
        public static async Task<IActionResult> UpdateGolfer(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "golfer/{id}")] HttpRequest req, ILogger log, int id)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<Shared.Models.Golfer>(requestBody.Replace("\u0022", "\""));
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Update Golfer Set FirstName = @FirstName, LastName = @LastName, Email = @Email, Handicap = @Handicap Where Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FirstName", input.FirstName);
                    command.Parameters.AddWithValue("@LastName", input.LastName);
                    command.Parameters.AddWithValue("@Email", input.Email);
                    command.Parameters.AddWithValue("@Handicap", input.Handicap);
                    command.Parameters.AddWithValue("@Id", id);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
            return new OkResult();
        }

        [FunctionName("GetGolfers")]
        public static async Task<IActionResult> GetGolfers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "golfer")] HttpRequest req, ILogger log)
        {
            List<Shared.Models.Golfer> golfers = new List<Shared.Models.Golfer>();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Select * from Golfer";
                    SqlCommand command = new SqlCommand(query, connection);
                    var reader = await command.ExecuteReaderAsync();
                    while (reader.Read())
                    {
                        Shared.Models.Golfer golfer = new Shared.Models.Golfer()
                        {
                            Id = (int)reader["Id"],
                            FirstName = reader["FirstName"].ToString(),
                            LastName = reader["LastName"].ToString(),
                            Email = reader["Email"].ToString(),
                            Handicap = Convert.ToInt16(reader["Handicap"]),
                            CreatedOn = (DateTime)reader["CreatedOn"],
                            
                        };
                        golfers.Add(golfer);
                    }
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
            if (golfers.Count > 0)
            {
                return new OkObjectResult(golfers);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [FunctionName("GetGolferById")]
        public static async Task<IActionResult> GetGolferById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "golfer/{id}")] HttpRequest req, ILogger log, int id)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlConnection connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString")))
                {
                    connection.Open();
                    var query = @"Select * from Golfer Where Id = @Id";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);
                    SqlDataAdapter da = new SqlDataAdapter(command);
                    da.Fill(dt);
                }
            }
            catch (Exception e)
            {
                log.LogError(e.ToString());
            }
            if (dt.Rows.Count == 0)
            {
                return new NotFoundResult();
            }

            var golfer = new Shared.Models.Golfer
            {
                Id = (int)dt.Rows[0].ItemArray[0],
                FirstName = dt.Rows[0].ItemArray[1].ToString(),
                LastName = dt.Rows[0].ItemArray[2].ToString(),
                Email = dt.Rows[0].ItemArray[3].ToString(),
                Handicap = (int)dt.Rows[0].ItemArray[4],
                CreatedOn = (DateTime)dt.Rows[0].ItemArray[5]
            };

            return new OkObjectResult(golfer);
        }
    }
}
