using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Photos
{
    public class PhotosDownload
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<PhotosDownload> _logger;

        public PhotosDownload(BlobServiceClient blobServiceClient, ILogger<PhotosDownload> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        [Function("PhotosDownload")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "photos/{Id}")] HttpRequest req, 
            Guid id)
        {
            if (id == Guid.Empty)
            {
                return new BadRequestObjectResult("Please provide a photo ID in the request.");
            }

            _logger.LogInformation($"Downloading {id}...");

            BlobContainerClient blobContainerClient;

            if (req.Query["size"] == "sm")
            {
                _logger.LogInformation("Retreiving the small size.");
                blobContainerClient = _blobServiceClient.GetBlobContainerClient("photos-small");                
            }
            else if(req.Query["size"] == "md")
            {
                _logger.LogInformation("Retreiving the medium size.");
                blobContainerClient = _blobServiceClient.GetBlobContainerClient("photos-medium");
            }
            else
            {
                _logger.LogInformation("Retreiving the original size.");
                blobContainerClient = _blobServiceClient.GetBlobContainerClient("photos");            
            }

            var blobClient = blobContainerClient.GetBlobClient($"{id}.jpg");

            var blobDownloadInfo = await blobClient.DownloadAsync();

            _logger.LogInformation($"Retreiving {blobDownloadInfo.Value.ContentType}");

            return new FileStreamResult(blobDownloadInfo.Value.Content, blobDownloadInfo.Value.ContentType) 
            { 
                FileDownloadName = $"{id}.jpg"
            };
        }
    }
}
