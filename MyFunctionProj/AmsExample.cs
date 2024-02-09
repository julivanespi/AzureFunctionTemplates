using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace MyFunctionProj
{
    public class AmsExample
    {
        private readonly ILogger _logger;

        public AmsExample(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AmsExample>();
        }

        /*Declare Database Details*/
        private static string Host = "<DATABASE-HOSTNAME>";/*Database FQDN*/
        private static string User = "<USERNAME>";
        private static string Password = "<PASSWORD>";
        private static string DBname = "<HOSTNAME>";/*Database Name*/
        private static string Port = "<PORT>";/*Database Port*/

        [Function("AmsExample")]
        public HttpResponseData Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req)
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
                    using (var command = new NpgsqlCommand("select dbid, client_id, person_id from personw1 order by person_id limit 10", conn))
                    {

                        var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            _logger.LogInformation("\nReading user\n");
                            people.Add(new Person
                            {
                            DbId = reader.GetString(0),
                            ClientId = reader.GetString(1),
                            PersonId = reader.GetString(2)
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
    }

    public class Person
        {
            public string? DbId { get; set; }
            public string? ClientId { get; set; }
            public string? PersonId { get; set; }
        }
}
