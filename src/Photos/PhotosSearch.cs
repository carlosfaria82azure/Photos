using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Photos.Models;

namespace Photos
{
    public class PhotosSearch
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger<PhotosSearch> _logger;

        public PhotosSearch(CosmosClient cosmosClient, ILogger<PhotosSearch> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }

        [Function("PhotosSearch")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {
            var searchTerm = req.Query["searchTerm"];

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                _logger.LogError("Search term is missing in the request query string.");
                return new BadRequestObjectResult("Search term is missing in the request query string.");
            }

            var container = _cosmosClient.GetContainer("photos", "metadata");

            var sqlQueryText = $"SELECT * FROM c WHERE CONTAINS(c.description, '{searchTerm}')";
            var queryDefinition = new QueryDefinition(sqlQueryText);

            var query = container.GetItemQueryIterator<PhotoUploadModel>(queryDefinition);

            var results = new List<dynamic>();

            while (query.HasMoreResults)
            {
                var response = await query.ReadNextAsync();
                results.AddRange(response);
            }

            return new OkObjectResult(results);
        }
    }
}
