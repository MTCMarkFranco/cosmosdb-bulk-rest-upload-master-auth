using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Azure.Core;
using Azure.Identity;

class Program
{
    private static readonly string uri = "";
    private static readonly string database = "";
    private static readonly string container = "";

    private static readonly string primaryKey = "";

    static async Task Main(string[] args)
    {
        var documents = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "id", "10" }, { "customerField2", "Field Data 2" } },
            new Dictionary<string, string> { { "id", "11" }, { "customerField2", "Field Data 2" } } 
        };

        string date = DateTime.UtcNow.ToString("R");
        string resourceLink = $"dbs/{database}/colls/{container}";
       
        var client = new HttpClient();
        
        // Generate master type token
        string authToken = GenerateMasterToken("post", "docs", resourceLink, date, primaryKey);

        // URL-encode the token
        string encodedAuthToken = HttpUtility.UrlEncode(authToken);
        
         client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Accept", "application/json");
        client.DefaultRequestHeaders.Add("Authorization", encodedAuthToken);
        client.DefaultRequestHeaders.Add("x-ms-date", date);
        client.DefaultRequestHeaders.Add("x-ms-version", "2015-12-16");
        client.DefaultRequestHeaders.Add("x-ms-documentdb-partitionkey", "id");
        

        foreach (var doc in documents)
        {
            var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(doc), Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{uri}{resourceLink}/docs", content);
            Console.WriteLine($"{response.StatusCode} {await response.Content.ReadAsStringAsync()}");
        }
    }

    private static string GenerateMasterToken(string verb, string resourceType, string resourceLink, string date, string key)
    {
        var keyBytes = Convert.FromBase64String(key);
        string text = $"{verb.ToLowerInvariant()}\n{resourceType.ToLowerInvariant()}\n{resourceLink}\n{date.ToLowerInvariant()}\n\n";
        var hmacSha256 = new HMACSHA256(keyBytes);
        var hashPayload = hmacSha256.ComputeHash(Encoding.UTF8.GetBytes(text));
        string signature = Convert.ToBase64String(hashPayload);
        return $"type=master&ver=1.0&sig={signature}";
    }

}