# Project Overview

This application is designed to resize images using Azure Functions. It utilizes the isolated process model for Azure Functions, which is recommended for new development according to Microsoft.

# Features

- Uploads photos to Azure Blob Storage.
- Creates resized copies of uploaded photos in separate containers.
- Searches photos based on descriptions using Azure Cosmos DB.
- Downloads photos by their unique identifier.
- (Optional) Integrates with Azure Computer Vision service to extract additional image data.

# Prerequisites

- .NET SDK (https://dotnet.microsoft.com/en-us/download/dotnet-framework)

# Deployment (Optional)

TODO

# Getting Started

## Installation

- Install the Azure Storage Emulator and Azure Cosmos DB Emulator following the instructions from these resources:
    - Azure Storage Emulator: https://learn.microsoft.com/en-us/azure/storage/common/storage-use-emulator
    - Azure Cosmos DB Emulator: https://learn.microsoft.com/en-us/azure/cosmos-db/how-to-develop-emulator

## Configuration

- Edit the local.settings.json file with the following connection strings and keys:
    - "AzureWebJobsStorage": "UseDevelopmentStorage=true" (Connects to local storage emulator)
    - "CosmosDBConnection": "<Your Cosmos DB connection string>"
    - "VisionKey": "<Your Azure Computer Vision service key>" (Optional)
    - "VisionEndpoint": "<Your Azure Computer Vision service endpoint>" (Optional)

## Manual Testing

- Upload a photo using an HTTP call with the following structure:
```
JSON
POST http://localhost:7198/api/PhotosStorage
Body:
{
  "name": "beagle.jpg",
  "description": "A lovely beagle dog",
  "tags": [
    "beagle",
    "dog",
    "lovely"
  ],
  "photo": "<Base64 encoded photo data>"
}
```

- Search for photos using an HTTP call with a query string:
```
GET http://localhost:7198/api/PhotosSearch?searchTerm={search term}
```

- Download a photo using an HTTP call with the photo ID:
```
GET http://localhost:7198/api/photos/{guid photo id}
```

## Connecting to Azure Computer Vision (Optional)

If you want to extract additional information about uploaded photos, you can integrate the Azure Computer Vision service. Replace the placeholder values in local.settings.json with your obtained keys and endpoint from the Azure portal.