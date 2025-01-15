using System.ComponentModel;
using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

class Program
{
    static async Task Main(string[] args)
    {
        
        IConfiguration Configuration;
        var client = new HttpClient();

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
        string partionKey = "part1";
        
        
        // Generate key-based Authentication token
        string keyAuthToken = GenerateMasterKeyAuthorizationSignature(HttpMethod.Post, "docs", resourceLink, date, primaryKey);

        // Sample Data records
        var documents = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "partition", partionKey }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data " } },
            new Dictionary<string, string> { { "partition", partionKey }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } },
            new Dictionary<string, string> { { "partition", partionKey }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } },
            new Dictionary<string, string> { { "partition", partionKey }, { "id", Guid.NewGuid().ToString() }, { "customerField2", "Field Data" } }
        };
        
        // Create http Headers for the /docs endpoint        
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("authorization", keyAuthToken);
        client.DefaultRequestHeaders.Add("x-ms-date", date);
        client.DefaultRequestHeaders.Add("x-ms-version", "2018-12-31");
        client.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", JsonConvert.SerializeObject(new[] { partionKey }));

        // Initialize Threading
        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(4); // Limit to 4 concurrent threads
                
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
                    var response = await client.PostAsync($"{uri}{resourceLink}/docs", new StringContent(content));
                    response.EnsureSuccessStatusCode();
                    Console.WriteLine($"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception occurred: {ex.Message}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                        Console.WriteLine($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                    }
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