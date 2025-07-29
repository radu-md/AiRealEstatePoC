using AiRealEstate.Core.Services;
using AiRealEstate.Core.Skills;
using AiRealEstate.Infrastructure.Services;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Load config
var config = builder.Configuration.GetSection("AzureOpenAI");
var deploymentName = config["DeploymentName"];
var endpoint = config["Endpoint"];
var apiKey = config["ApiKey"];

// Register Semantic Kernel
builder.Services.AddSingleton<Kernel>(_ =>
{
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);
    return kernelBuilder.Build();
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
app.UseAuthorization();
app.MapControllers();

app.Run();
