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

## Partitioning in CosmosDB

### How Partitioning Works

In CosmosDB, data is partitioned to optimize performance and scalability. Each item in the database is assigned to a partition based on a partition key. The partition key is a property within your data that CosmosDB uses to distribute the data across multiple partitions.

### Specifying Partition Key in Data

When you create a container in CosmosDB, you specify a partition key. Each record (item) in your data must include this partition key. For example, if your partition key is `PartitionKey`, each item must have a `PartitionKey` property.

### Example Data with Partition Key

```json
{
    "id": "1",
    "PartitionKey": "partition1",
    "name": "Item 1",
    "description": "Description for Item 1"
}
```

### Matching Partition Key Header

When performing bulk uploads, ensure that the partition key in your data matches the partition key specified in the container. This is crucial for the data to be correctly partitioned.

### Setting Partition Key in Code

In the bulk upload logic, you specify the partition key when creating items:

```csharp
// ...existing code...
foreach (var item in items)
{
    await container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
}
// ...existing code...
```

## Unique ID Requirement

### Ensuring Unique IDs

Each item in a CosmosDB container must have a unique `id` property. This `id` is used to uniquely identify each item within a partition.

### Example of Unique ID

```json
{
    "id": "1",
    "PartitionKey": "partition1",
    "name": "Item 1",
    "description": "Description for Item 1"
}
```

Ensure that the `id` property is unique across all items in the container to avoid conflicts and ensure data integrity.
