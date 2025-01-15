# CosmosDB Bulk REST Upload

This project demonstrates how to perform bulk uploads to CosmosDB using REST API with authentication.

## Program.cs Breakdown

Below is a breakdown of the `Program.cs` file, explaining each section of the code.

### Main Method

The entry point of the application.

```csharp
// ...existing code...
public static void Main(string[] args)
{
    // ...existing code...
    var builder = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets<Program>(); // Add user secrets
    var configuration = builder.Build();
    // ...existing code...
}
```

### Configuration Setup

Setting up configuration sources including JSON file and user secrets.

```csharp
// ...existing code...
var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>(); // Add user secrets
var configuration = builder.Build();
// ...existing code...
```

### CosmosDB Client Initialization

Initializing the CosmosDB client with the configuration settings.

```csharp
// ...existing code...
var cosmosClient = new CosmosClient(configuration["CosmosDB:Endpoint"], configuration["CosmosDB:Key"]);
// ...existing code...
```

### Bulk Upload Logic

The core logic for performing bulk uploads to CosmosDB.

```csharp
// ...existing code...
foreach (var item in items)
{
    await container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
}
// ...existing code...
```

## Adding .NET User Secrets

To securely store sensitive information such as CosmosDB keys, you can use .NET user secrets. Follow the steps below to add user secrets to your project.

### Initialize User Secrets

Run the following command in the project directory to initialize user secrets:

```sh
dotnet user-secrets init
```

### Add User Secrets

Use the following command to add user secrets by name:

```sh
dotnet user-secrets set "CosmosDB:Endpoint" "<your-cosmosdb-endpoint>"
dotnet user-secrets set "CosmosDB:Key" "<your-cosmosdb-key>"
dotnet user-secrets set "CosmosDB:DatabaseId" "<your-cosmosdb-database-id>"
dotnet user-secrets set "CosmosDB:ContainerId" "<your-cosmosdb-container-id>"
```

These secrets will be stored in a secure location on your machine and can be accessed in the code as shown in the `Program.cs` file.
