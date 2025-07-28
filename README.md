# 🏡 AiRealEstatePoC

A proof-of-concept application demonstrating an **AI-powered real estate assistant** built with **ASP.NET Core 9**, **Azure OpenAI (GPT-4o)**, and **Semantic Kernel**.

This project simulates a conversational agent that helps users find properties on [romimo.ro](https://www.romimo.ro) by understanding natural language and extracting preferences such as location, budget, and amenities.

---

## 📁 Table of Contents
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Building and Running](#building-and-running)
- [API Usage](#api-usage)
- [API Reference](#api-reference)
- [Testing](#testing)
- [Prompts](#prompts)

---

<a id="project-structure"></a>
## 🧱 Project Structure

```
AiRealEstatePoC/
├── AiRealEstate.Api/           # ASP.NET Core Web API (entry point)
├── AiRealEstate.Core/          # Core models, prompts, and service interfaces
├── AiRealEstate.Infrastructure/ # Scraping + query builder implementations
├── AiRealEstate.WebApp/        # Minimal JavaScript chat-based frontend
├── AiRealEstate.Tests/         # Unit and integration tests
```

- **`AiRealEstate.Api`**: Exposes `/api/chat` for conversational AI; integrates Semantic Kernel with Azure OpenAI.
- **`AiRealEstate.Core`**: Shared domain: `ChatResult`, `UserPreferences`, prompt templates, and service contracts.
- **`AiRealEstate.Infrastructure`**: Logic for scraping listings from romimo.ro and building dynamic search URLs.
- **`AiRealEstate.WebApp`**: Simple front-end UI for chat interaction.
- **`AiRealEstate.Tests`**: Test coverage for logic, conversation flow, and prompt extraction.

---

<a id="prerequisites"></a>
## ⚙️ Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- Azure OpenAI resource (deployed model: e.g., `gpt-4o`)
- Optional: [Visual Studio 2022+](https://visualstudio.microsoft.com/) with ASP.NET workload

---

<a id="configuration"></a>
## 🛠️ Configuration

In `src/AiRealEstate.Api/appsettings.json`, configure your Azure OpenAI settings:

```json
{
  "AzureOpenAI": {
    "DeploymentName": "gpt-4o",
    "Endpoint": "https://<your-resource-name>.openai.azure.com/",
    "ApiKey": "<your-api-key>"
  }
}
```

---

<a id="building-and-running"></a>
## 🚀 Building and Running

```bash
# From repository root:
dotnet build AiRealEstatePoC.sln

# Run the API backend:
dotnet run --project src/AiRealEstate.Api

# Then open: http://localhost:<port>
```

To run the frontend (static HTML + JS):

```bash
# Serve AiRealEstate.WebApp using any static file server
cd src/AiRealEstate.WebApp
# Example with .NET dev server:
dotnet serve -d
```

---

<a id="api-usage"></a>
## 📡 API Usage

Explore endpoints with Swagger at:

```
http://localhost:<port>/swagger
```

### Primary endpoint

- `POST /api/chat`  
  Accepts `{ "message": "..." }` and returns AI response, extracted preferences, and suggested follow-up questions.

---

<a id="api-reference"></a>
## 📝 API Reference

Detailed API documentation is available in [`src/AiRealEstate.Api/README_API.md`](src/AiRealEstate.Api/README_API.md).

---

<a id="testing"></a>
## 🧪 Testing

Run all unit and integration tests:

```bash
dotnet test
```

---

<a id="prompts"></a>
## 💬 Prompts

Prompt templates used to extract preferences from natural language can be found in:

[`src/AiRealEstate.Core/Prompts/PROMPTS.md`](src/AiRealEstate.Core/Prompts/PROMPTS.md)

These are passed to Semantic Kernel to generate structured `UserPreferences`.

---

<a id="acknowledgements"></a>
## 🙌 Acknowledgements

- [Azure OpenAI Service](https://azure.microsoft.com/en-us/products/ai-services/openai-service)
- [Microsoft Semantic Kernel](https://github.com/microsoft/semantic-kernel)
- [romimo.ro](https://www.romimo.ro) for real estate data structure