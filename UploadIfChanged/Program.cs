using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Container = Microsoft.Azure.Cosmos.Container;

namespace UploadIfChanged
{
    class Program
    {
        // Replace these with your Cosmos DB credentials
        private const string EndpointUri = "https://your-cosmos-account.documents.azure.com:443/";
        private const string PrimaryKey = "your-primary-key";
        private const string DatabaseId = "YourDatabase";
        private const string ContainerId = "YourContainer";

        private static CosmosClient _cosmosClient;
        private static Container _container;

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Starting Cosmos DB change detection demo...");

                // Initialize Cosmos Client
                _cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
                _container = _cosmosClient.GetContainer(DatabaseId, ContainerId);

                // Create sample product (or get from your data source)
                var product = new Product
                {
                    Id = "prod-123",
                    Name = "Wireless Mouse",
                    Price = 24.99m,
                    Category = "Electronics",
                    LastUpdated = DateTime.UtcNow
                };

                Console.WriteLine("First upload (should create new document)...");
                bool wasUpdated1 = await UpsertIfChangedAsync(product, product.Category);
                Console.WriteLine($"Update performed: {wasUpdated1}");

                Console.WriteLine("\nSecond upload with same data (should skip)...");
                bool wasUpdated2 = await UpsertIfChangedAsync(product, product.Category);
                Console.WriteLine($"Update performed: {wasUpdated2}");

                Console.WriteLine("\nThird upload with changed data (should update)...");
                product.Price = 22.99m; // Change the price
                product.LastUpdated = DateTime.UtcNow;
                bool wasUpdated3 = await UpsertIfChangedAsync(product, product.Category);
                Console.WriteLine($"Update performed: {wasUpdated3}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        public static async Task<bool> UpsertIfChangedAsync<T>(T item, string partitionKeyValue) where T : class
        {
            try
            {
                // Get the ID from the item (assumes your model has an Id property)
                string id = item.GetType().GetProperty("Id").GetValue(item).ToString();

                // Try to read the existing item
                ItemResponse<T> existingItemResponse = await _container.ReadItemAsync<T>(
                    id: id,
                    partitionKey: new PartitionKey(partitionKeyValue));

                var existingItem = existingItemResponse.Resource;

                // Compare with new item
                if (ItemsEqual(existingItem, item))
                {
                    Console.WriteLine("No changes detected - skipping upload");
                    return false;
                }

                // If different, update with the new item
                await _container.ReplaceItemAsync(
                    item: item,
                    id: id,
                    partitionKey: new PartitionKey(partitionKeyValue),
                    requestOptions: new ItemRequestOptions { IfMatchEtag = existingItemResponse.ETag });

                Console.WriteLine("Document updated with changes");
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Item doesn't exist - create it
                await _container.CreateItemAsync(item, new PartitionKey(partitionKeyValue));
                Console.WriteLine("New document created");
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.PreconditionFailed)
            {
                Console.WriteLine("ETag mismatch - document was modified by another process");
                throw;
            }
        }

        private static bool ItemsEqual<T>(T item1, T item2)
        {
            // Simple JSON comparison - you may want a more sophisticated comparison
            return JsonConvert.SerializeObject(item1) == JsonConvert.SerializeObject(item2);
        }
    }

    public class Product
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

