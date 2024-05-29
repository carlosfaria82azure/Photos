using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Photos.AnalyzerService;
using Photos.AnalyzerService.Abstractions;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var cosmosClientOptions = new CosmosClientOptions
        {
            HttpClientFactory = () => new HttpClient(handler)
        };

        services.AddSingleton(x => new BlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage")));
        services.AddSingleton(x => new CosmosClient(Environment.GetEnvironmentVariable("CosmosDBConnection"), cosmosClientOptions));
        services.AddSingleton<IAnalyzerService, ComputerVisionAnalyzerService>();
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
