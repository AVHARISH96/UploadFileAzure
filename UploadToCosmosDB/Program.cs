using Microsoft.Azure.Cosmos;
using Container = Microsoft.Azure.Cosmos.Container;

class Program
{
    private static readonly string EndpointUri = "";
    private static readonly string PrimaryKey = "";
    private static readonly string DatabaseId = "";
    private static readonly string ContainerId = "";

    static async Task Main(string[] args)
    {
        try
        {
            CosmosClient cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            Database database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(ContainerId, "/id");

            string filePath = "path_to_your_file.txt";
            string fileContent = await File.ReadAllTextAsync(filePath);

            var fileDocument = new
            {
                id = Guid.NewGuid().ToString(),
                fileName = Path.GetFileName(filePath),
                content = fileContent,
                uploadedOn = DateTime.UtcNow
            };

            var response = await container.CreateItemAsync(fileDocument);
            Console.WriteLine($"Uploaded document with ID: {response.Resource.id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}