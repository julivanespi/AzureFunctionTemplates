using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MyFunctionProj
{
    public class HttpExample
    {
        private readonly ILogger _logger;

        public HttpExample(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpExample>();
        }

        /*Declare Database Details*/
        private static string Host = "<DATABASE-HOSTNAME>";/*Database FQDN*/
        private static string User = "<USERNAME>";
        private static string Password = "<PASSWORD>";
        private static string DBname = "<HOSTNAME>";/*Database Name*/
        private static string Port = "<PORT>";/*Database Port*/

        [Function("HttpExampleJulio")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            string connString = String.Format(
                    "Server={0}; User Id={1}; Database={2}; Port={3}; Password={4};SSLMode=Prefer",
                    Host,
                    User,
                    DBname,
                    Port,
                    Password);

                    /*Connecting to PostgreSQL*/
            using (var conn = new NpgsqlConnection(connString))
            {
                _logger.LogInformation("Opening connection");
                _logger.LogInformation(connString);
                conn.Open();
                _logger.LogInformation("Opening connection using user creds....");

                var people = new List<Person>();
                /*Query the Database */
                    using (var command = new NpgsqlCommand("SELECT * from person", conn))
                    {

                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            _logger.LogInformation("\nReading user\n");
                            people.Add(new Person
                            {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            email = reader.GetString(2),
                            city = reader.GetString(3),
                            state = reader.GetString(4)
                            });
                        }

                        var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

                        // Serialize and write response
                        response.WriteString(JsonSerializer.Serialize(people));
                        return response;
                    }
            }

        }

        public class Person
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string email { get; set; }
            public string city { get; set; }
            public string state { get; set; }
        }
    }
}
