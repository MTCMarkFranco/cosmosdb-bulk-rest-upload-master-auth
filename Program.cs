using System.ComponentModel;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        
        IConfiguration Configuration;
        

        var builder = new ConfigurationBuilder()
            .AddUserSecrets<Program>();
        Configuration = builder.Build();
    
        // Initializing global vars
        string uri = Configuration["CosmosDb:Uri"] ?? throw new ArgumentNullException("CosmosDb:Uri");
        string database = Configuration["CosmosDb:Database"] ?? throw new ArgumentNullException("CosmosDb:Database");
        string container = Configuration["CosmosDb:Container"] ?? throw new ArgumentNullException("CosmosDb:Container");
        string primaryKey = Configuration["CosmosDb:PrimaryKey"] ?? throw new ArgumentNullException("CosmosDb:PrimaryKey");
        string date = DateTime.UtcNow.ToString("R");
        string resourceLink = $"dbs/{database}/colls/{container}";
        
        // Generate key-based Authentication token
        string keyAuthToken = GenerateMasterKeyAuthorizationSignature(HttpMethod.Post, "docs", resourceLink, date, primaryKey);

        // Sample Data records
        var documents = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "partition", "part1" }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data " } },
            new Dictionary<string, string> { { "partition", "part1" }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } },
            new Dictionary<string, string> { { "partition", "part2" }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } },
            new Dictionary<string, string> { { "partition", "part2" }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } }
        };
        
                
        // Initialize Threading
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(4); // Limit to 4 concurrent threads

        using var client = new HttpClient(); // Create a single HttpClient instance

        // Loop through all Records and Post to CosmosDB using Semaphore
        foreach (var doc in documents)
        {
            await semaphore.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(new Random().Next(0, 100)); // add some entropy to simulate network latency

                    var content = Newtonsoft.Json.JsonConvert.SerializeObject(doc);

                    // Create http Headers for the /docs endpoint        
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{uri}{resourceLink}/docs")
                    {
                        Content = new StringContent(content)
                    };
                    request.Headers.Clear();
                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("authorization", keyAuthToken);
                    request.Headers.Add("x-ms-date", date);
                    request.Headers.Add("x-ms-version", "2018-12-31");
                    request.Headers.Add("x-ms-documentdb-partitionkey", JsonConvert.SerializeObject(new[] { doc["partition"].ToString() }));

                    var response = await client.SendAsync(request);

                    response.EnsureSuccessStatusCode();

                    Console.WriteLine($"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("All tasks have completed!");
    
    }

    private static string GenerateMasterKeyAuthorizationSignature(HttpMethod verb, string resourceType, string resourceLink, string date, string key)
    {
        var keyType = "master";
        var tokenVersion = "1.0";
        var payload = $"{verb.ToString().ToLowerInvariant()}\n{resourceType.ToLowerInvariant()}\n{resourceLink}\n{date.ToLowerInvariant()}\n\n";

        var hmacSha256 = new System.Security.Cryptography.HMACSHA256 { Key = Convert.FromBase64String(key) };
        var hashPayload = hmacSha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToBase64String(hashPayload);
        var authSet = WebUtility.UrlEncode($"type={keyType}&ver={tokenVersion}&sig={signature}");

        return authSet;
    }

}