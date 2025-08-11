using AiRealEstate.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace AiRealEstate.Core.Skills;

public class ExtractUserPreferencesSkill : IExtractUserPreferencesSkill
{
    private readonly Kernel _kernel;
    private readonly string _promptTemplate;

    public ExtractUserPreferencesSkill(Kernel kernel)
    {
        _kernel = kernel;
        var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "ExtractUserPreferencesPrompt.txt");
        _promptTemplate = File.ReadAllText(promptPath);
    }

    public async Task<UserPreferences> ExtractAsync(string aiModel, string userMessage)
    {
        var prompt = _promptTemplate.Replace("{{userMessage}}", userMessage);

        var history = new ChatHistory();
        history.AddUserMessage(prompt);

        var completion = await _kernel
            .GetRequiredService<IChatCompletionService>(aiModel)
            .GetChatMessageContentAsync(history);

        try
        {
            var prefs = JsonSerializer.Deserialize<UserPreferences>(completion.Content!, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            // Replace any property value containing "..." with an empty string
            if (prefs != null)
            {
                if (prefs.PropertyType is not null && prefs.PropertyType.Contains("..."))
                    prefs.PropertyType = "";
                if (prefs.TransactionType is not null && prefs.TransactionType.Contains("..."))
                    prefs.TransactionType = "";
                if (prefs.County is not null && prefs.County.Contains("..."))
                    prefs.County = "";
                if (prefs.City is not null && prefs.City.Contains("..."))
                    prefs.City = "";
                if (prefs.TextFilter is not null && prefs.TextFilter.Contains("..."))
                    prefs.TextFilter = "";
            }

            return prefs ?? new();
        }
        catch
        {
            return new();
        }
    }
}
