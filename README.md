# AiRealEstatePoC

A proof-of-concept application demonstrating an AI-powered real estate assistant using Azure OpenAI and the Semantic Kernel.

## Table of Contents
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Building and Running](#building-and-running)
- [API Usage](#api-usage)
- [Testing](#testing)
- [Prompts](#prompts)


## Project Structure
- **AiRealEstate.Api**: ASP.NET Core Web API providing chat and listing endpoints. Integrates Azure OpenAI via Semantic Kernel.
- **AiRealEstate.Core**: Core models, prompts, and service interfaces (chat service, query builder, conversation state).
- **AiRealEstate.Infrastructure**: Implementations for scraping real estate listings and building AI queries.
- **AiRealEstate.WebApp**: Front-end web application consuming the API (e.g., React or Razor Pages).
- **AiRealEstate.Tests**: Unit and integration tests for core and API projects.

## Prerequisites
- .NET 9 SDK (https://dotnet.microsoft.com/download)
- An Azure OpenAI resource with deployment configured (e.g., `gpt-4o`).

## Configuration
1. In `src/AiRealEstate.Api/appsettings.json`, add your Azure OpenAI credentials:
   ```json
   {
     "AzureOpenAI": {
       "DeploymentName": "<your-deployment-name>",
       "Endpoint": "https://your-openai-endpoint.azure.com/",
       "ApiKey": "<your-api-key>"
     }
   }
   ```

## Building and Running
```bash
# From the solution root
dotnet build AiRealEstatePoC.sln
dotnet run --project src/AiRealEstate.Api
```

## API Usage
Once running, navigate to `http://localhost:<port>/swagger` to explore the API endpoints and test them interactively.

## Testing
Run all tests:
```bash
dotnet test
```

## Prompts
A list of AI prompt templates used by the application can be found in [`src/AiRealEstate.Core/Prompts/PROMPTS.md`](src/AiRealEstate.Core/Prompts/PROMPTS.md).
