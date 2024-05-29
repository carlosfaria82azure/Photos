using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Photos.AnalyzerService.Abstractions;
using Photos.Models;
using System.Text.Json;

namespace Photos
{
    public class PhotosStorage
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly CosmosClient _cosmosClient;
        private readonly IAnalyzerService _analyzerService;
        private readonly ILogger<PhotosStorage> _logger;

        public PhotosStorage(BlobServiceClient blobServiceClient, CosmosClient cosmosClient, IAnalyzerService analyzerService, ILogger<PhotosStorage> logger)
        {
            _blobServiceClient = blobServiceClient;
            _cosmosClient = cosmosClient;
            _analyzerService = analyzerService;
            _logger = logger;
        }

        [Function(nameof(PhotosStorage))]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var photoUploadModel = JsonConvert.DeserializeObject<PhotoUploadModel>(requestBody);

            if (string.IsNullOrEmpty(photoUploadModel.Photo))
            {
                _logger.LogError($"Photo is missing in the request body.");
                return new BadRequestObjectResult($"Photo is missing in the request body.");
            }

            var newId = Guid.NewGuid();
            var blobName = $"{newId}.jpg";

            var containerClient = _blobServiceClient.GetBlobContainerClient("photos");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            var blobClient = containerClient.GetBlobClient(blobName);
            var bytes = Convert.FromBase64String(photoUploadModel.Photo);

            using var stream = new MemoryStream(bytes);
            await blobClient.UploadAsync(stream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" } });

            stream.Position = 0;
            var analysisResult = await _analyzerService.AnalyzeAsync(stream);

            var item = new
            {
                id = newId,
                name = photoUploadModel.Name,
                description = photoUploadModel.Description,
                tags = photoUploadModel.Tags,
                analysis = analysisResult
            };

            var database = await _cosmosClient.CreateDatabaseIfNotExistsAsync("photos");
            var container = await database.Database.CreateContainerIfNotExistsAsync("metadata", "/id");
            await container.Container.CreateItemAsync(item);

            _logger.LogInformation($"Successfully uploaded {newId}.jpg file and its metadata");
            return new OkObjectResult(newId);
        }
    }
}
