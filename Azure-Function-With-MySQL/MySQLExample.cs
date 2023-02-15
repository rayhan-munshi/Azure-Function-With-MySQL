using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MySqlConnector;
using System.Collections.Generic;

namespace Azure_Function_With_MySQL
{
    public class Person
    {
        public int Personid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }

    }
    public static class MySQLExample
    {
        static readonly MySqlConnection connection = new(Environment.GetEnvironmentVariable("MySQLConnectionStr"));
        [FunctionName("create-table")]
        public static async Task<IActionResult> CreateTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Persons (
                                    Personid int NOT NULL AUTO_INCREMENT,
                                    LastName varchar(255) NOT NULL,
                                    FirstName varchar(255),
                                    Age int,
                                    PRIMARY KEY (Personid)
                                );";
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();

            return new OkObjectResult("Persons table created!");
        }

        [FunctionName("drop-table")]
        public static async Task<IActionResult> DropTable(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var str = Environment.GetEnvironmentVariable("MySQLConnectionStr");

            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"DROP TABLE Persons;";
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();


            return new OkObjectResult("Persons table dropped!");
        }

        [FunctionName("insert-data")]
        public static async Task<IActionResult> InsertData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var str = Environment.GetEnvironmentVariable("MySQLConnectionStr");

            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `mytestdb`.`persons` (`LastName`, `FirstName`, `Age`) VALUES ('Rayhan', 'Tonmoy', '36');";
            await cmd.ExecuteNonQueryAsync();
            await connection.CloseAsync();


            return new OkObjectResult("Data inserted!");
        }

        [FunctionName("get-data")]
        public static async Task<IActionResult> GetData(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var str = Environment.GetEnvironmentVariable("MySQLConnectionStr");
            var personList = new List<Person>();
            await connection.OpenAsync();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM mytestdb.persons;";
            using var reader = await cmd.ExecuteReaderAsync();
            

            while (await reader.ReadAsync())
            {
                var person = new Person()
                {
                    Personid = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Age = reader.GetInt32(3),
                };
                personList.Add(person);
            }

            await connection.CloseAsync();


            return new OkObjectResult(personList);
        }
    }
}
