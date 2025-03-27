using Microsoft.Azure.Cosmos;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DownloadFileFromCosmosDB
{
    class Program
    {
        private static readonly string EndpointUri = "your-cosmosdb-endpoint";
        private static readonly string PrimaryKey = "your-primary-key";
        private static readonly string DatabaseId = "your-database-id";
        private static readonly string ContainerId = "your-container-id";

        static async Task Main(string[] args)
        {
            try
            {
                string documentId = "your-document-id";
                string filePath = "path-to-save-file.ext"; // Include extension

                using (CosmosClient client = new CosmosClient(EndpointUri, PrimaryKey))
                {
                    Container container = client.GetContainer(DatabaseId, ContainerId);

                    var response = await container.ReadItemAsync<dynamic>(documentId, new PartitionKey("your-partition-key"));

                    string base64Data = response.Resource.fileData;
                    byte[] fileBytes = Convert.FromBase64String(base64Data);

                    File.WriteAllBytes(filePath, fileBytes);

                    Console.WriteLine($"File downloaded successfully to {filePath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
