using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Photos
{
    public class PhotosResizer
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<PhotosResizer> _logger;

        public PhotosResizer(BlobServiceClient blobServiceClient, ILogger<PhotosResizer> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        [Function(nameof(PhotosResizer))]
        public async Task Run(
            [BlobTrigger("photos/{name}")] Stream stream,
            string name)
        {
            _logger.LogInformation("Resizing...");

            try
            {
                using var msMedium = CreateMemoryStream(stream, ImageSize.Medium);
                using var msSmall = CreateMemoryStream(stream, ImageSize.Small);

                await UploadResizedImage(msMedium, "photos-medium", name);
                await UploadResizedImage(msSmall, "photos-small", name);              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }

        private MemoryStream CreateMemoryStream(Stream image, ImageSize imageSize)
        {
            var memoryStream = new MemoryStream();
            image.Position = 0;
            var img = Image.Load(image);
            var desiredWidth = imageSize == ImageSize.Medium ? img.Width / 2 : img.Width / 4;
            var desiredHeight = imageSize == ImageSize.Medium ? img.Height / 2 : img.Height / 4;

            img.Mutate(ctx => ctx.Resize(desiredWidth, desiredHeight));

            img.SaveAsJpeg(memoryStream);

            memoryStream.Position = 0;
            return memoryStream;
        }

        private async Task UploadResizedImage(Stream image, string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);           

            await blobClient.UploadAsync(image, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "image/jpeg" } });
        }

        private enum ImageSize
        {
            Medium,
            Small
        }
    }
}
