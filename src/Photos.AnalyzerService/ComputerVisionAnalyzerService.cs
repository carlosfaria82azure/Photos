using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Configuration;
using Photos.AnalyzerService.Abstractions;

namespace Photos.AnalyzerService
{
    public class ComputerVisionAnalyzerService : IAnalyzerService
    {
        private readonly ComputerVisionClient _client;

        public ComputerVisionAnalyzerService(IConfiguration configuration)
        {
            var visionKey = configuration["VisionKey"];
            var visionEndpoint = configuration["VisionEndpoint"];
            _client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionKey))
            {
                Endpoint = visionEndpoint
            };
        }

        public async Task<dynamic> AnalyzeAsync(MemoryStream image)
        {           
            var imageAnalisis = await _client.AnalyzeImageInStreamAsync(image);
            var result = new
            {
                metadata = new
                {
                    width = imageAnalisis.Metadata.Width,
                    height = imageAnalisis.Metadata.Height,
                    format = imageAnalisis.Metadata.Format
                },
                categories = imageAnalisis.Categories.Select(s => s.Name).ToArray()
            };
            return result;
        }
    }
}
