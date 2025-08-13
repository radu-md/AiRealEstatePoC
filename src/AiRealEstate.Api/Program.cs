using AiRealEstate.Core.Infrastructure;
using AiRealEstate.Core.Services;
using AiRealEstate.Core.Skills;
using AiRealEstate.Infrastructure.Services;
using Azure.Identity;
using Google.Apis.Auth.OAuth2;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load config
AzureOpenAI azureOpenAiConfig = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAI>()
    ?? throw new Exception("AzureOpenAI section is missing or invalid in the configuration.");
VertexAI vertexAiConfig = builder.Configuration.GetSection("VertexAI").Get<VertexAI>()
            ?? throw new Exception("VertexAI section is missing or invalid in the configuration.");

// Register Semantic Kernel
builder.Services.AddSingleton<Kernel>(_ =>
{
    var kb = Kernel.CreateBuilder();

    kb.AddAzureOpenAIChatCompletion(
        deploymentName: azureOpenAiConfig.DeploymentName,
        endpoint: azureOpenAiConfig.Endpoint,
        apiKey: azureOpenAiConfig.ApiKey,
        serviceId: "azure"
    );

    // === Vertex AI ===
    var saJson = Encoding.UTF8.GetString(Convert.FromBase64String(vertexAiConfig.ServiceAccountBase64));
    GoogleCredential cred = GoogleCredential.FromJson(saJson);
    cred = cred.CreateScoped("https://www.googleapis.com/auth/cloud-platform");
    string? cachedToken = null;
    DateTimeOffset expiresAt = DateTimeOffset.MinValue;

    async ValueTask<string> TokenProvider()
    {
        var now = DateTimeOffset.UtcNow;
        if (cachedToken != null && now < expiresAt.AddMinutes(-2)) 
            return cachedToken;

        var tok = await cred.UnderlyingCredential.GetAccessTokenForRequestAsync();
        cachedToken = tok;
        expiresAt = now.AddMinutes(45);
        return tok;
    }

    kb.AddVertexAIGeminiChatCompletion(
        modelId: vertexAiConfig.Model,          
        bearerTokenProvider: TokenProvider,
        location: vertexAiConfig.Location,      
        projectId: vertexAiConfig.ProjectId,
        apiVersion: VertexAIVersion.V1,
        serviceId: "vertex");

    return kb.Build();
});

builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSingleton<IConversationStateService, ConversationStateService>();
builder.Services.AddSingleton<IUserPreferencesStateService, UserPreferencesStateService>();
builder.Services.AddScoped<IQueryBuilderService, QueryBuilderService>();
builder.Services.AddScoped<IExtractUserPreferencesSkill, ExtractUserPreferencesSkill>();
builder.Services.AddScoped<IListingScraperService, ListingScraperService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Add Swagger services
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

var app = builder.Build();
// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

app.MapGet("/health", () =>
                Results.Json(new { status = "OK", timestamp = DateTime.UtcNow })
            )
            .WithName("HealthCheck")
            .WithTags("System");

app.MapGet("/config", (IConfiguration cfg) =>
    {
        bool hasAoai = !string.IsNullOrWhiteSpace(cfg["AzureOpenAI:ApiKey"]);
        bool hasVtx = !string.IsNullOrWhiteSpace(cfg["VertexAI:ProjectId"]);
        string? cred = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
        return Results.Json(new { hasAoai, hasVtx, hasGoogleCredPath = !string.IsNullOrEmpty(cred) });
    })
    .WithName("ConfigCheck")
    .WithTags("System");

// Redirect root to chat UI
app.MapGet("/", () => Results.Redirect("/chat/index.html"));

app.Run();