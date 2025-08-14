using AiRealEstate.Core.Models;
using AiRealEstate.Core.Skills;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using System.Text;

namespace AiRealEstate.Core.Services;

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly IConversationStateService _state;
    private readonly IQueryBuilderService _queryBuilder;
    private readonly IExtractUserPreferencesSkill _extractUserPreferencesSkill;
    private readonly IUserPreferencesStateService _userPreferencesStateService;
    private readonly IListingScraperService _listingScraperService;

    public ChatService(
        Kernel kernel, 
        IConversationStateService state, 
        IQueryBuilderService queryBuilder, 
        IExtractUserPreferencesSkill extractUserPreferencesSkill,
        IUserPreferencesStateService userPreferencesStateService,
        IListingScraperService listingScraperService
        )
    {
        _kernel = kernel;
        _state = state;
        _queryBuilder = queryBuilder;
        _extractUserPreferencesSkill = extractUserPreferencesSkill;
        _userPreferencesStateService = userPreferencesStateService;
        _listingScraperService = listingScraperService;
    }

    public async Task<ChatResult> GetResponseAsync(string aiModel, string sessionId, string? newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
        {
            return new ChatResult { Response = "Te rog să introduci un mesaj." };
        }

        if (newMessage.Contains("Reseteaza", StringComparison.InvariantCultureIgnoreCase))
        {
            _state.RemoveAllMessages(sessionId);
            _userPreferencesStateService.ResetPreferences(sessionId);

            return new ChatResult
            {
                Response = "Preferințele tale au fost resetate. Te rog să începi o nouă căutare."
            };
        }

        var chat = _kernel.GetRequiredService<IChatCompletionService>(aiModel);
        var pastMessages = _state.GetHistory(sessionId);
        var userPreferences = await _extractUserPreferencesSkill.ExtractAsync(aiModel, newMessage);
        var newUserPreferences = _userPreferencesStateService.UpdatePreferences(sessionId, userPreferences);

        var history = new ChatHistory();
        history.AddSystemMessage("""
        Ești un asistent imobiliar inteligent. Răspunzi în limba română și ajuți utilizatorii să găsească proprietăți potrivite.
        Obiectivul tău este să înțelegi preferințele utilizatorului (ex: tipul locuinței, orașul, zona, prețul, facilități dorite) și să oferi sugestii clare și relevante.
        Blochează orice conținut inadecvat sau necorespunzător.
        Limitează-te la sugestii scurte și la obiect de maxim 100 de caractere.
        """);

        if (!newUserPreferences.IsEmpty())
        {
            history.AddSystemMessage($"Preferinte pentru sugestii: {newUserPreferences.ToString()}");
        }

        AddPastMessagesToHistory(history, pastMessages);

        history.AddUserMessage(newMessage);

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var assistantReply = await chat.GetChatMessageContentAsync(history);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;

        if (assistantReply is null || string.IsNullOrWhiteSpace(assistantReply.Content))
        {
            return new ChatResult { Response = "Îmi pare rău, nu am putut genera un răspuns." };
        }


        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "user",
            Content = newMessage
        });
        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "assistant",
            Content = assistantReply.Content
        });

        var adUrl = await _queryBuilder.BuildUrlAsync(newUserPreferences);

        string inputText = string.Join(" ", history
            .Select(m => m.Content));


        var suggestedQuestions = GenerateSmartSuggestions(newUserPreferences);
        if (suggestedQuestions is not null && suggestedQuestions.Any())
        {
            history.AddSystemMessage($"Întrebări sugerate: {String.Join(" ", suggestedQuestions)}");
        }

        if (string.IsNullOrWhiteSpace(adUrl))
        {
            return new ChatResult
            {
                Response = assistantReply.Content,
                SuggestedQuestions = suggestedQuestions,
                RequestCost = CalculateRequestCost(aiModel: aiModel, inputText: inputText, outputText: assistantReply.Content, elapsedMs: elapsedMs)
            };
        }

        var listing = await _listingScraperService.ExtractListingFromUrlAsync(adUrl);
        var listings = listing is not null ? new List<Listing> { listing } : null;

        return new ChatResult
        {
            Response = assistantReply.Content,
            Listings = listings,
            SuggestedQuestions = suggestedQuestions,
            RequestCost = CalculateRequestCost(aiModel: aiModel, inputText: inputText, outputText: assistantReply.Content, elapsedMs: elapsedMs)
        };
    }

    private RequestCost CalculateRequestCost(string aiModel, string? inputText, string outputText, long elapsedMs)
    {
        /*
            GPT-5-nano (Azure OpenAI)
            romanian words: 1 token = 0.75 words
            $0.05 / 1M tokens
            $0.40 / 1M tokens

            gemini-2.0-flash-lite (Vertex AI)
            romanian words: 1 token = 0.75 words
            $0.10 / 1M tokens
            $0.40 / 1M tokens
        */
        if (string.IsNullOrWhiteSpace(inputText) || string.IsNullOrWhiteSpace(outputText))
        {
            return new RequestCost();
        }

        var inputPrice = aiModel == "azure" ? 0.05 : 0.10; // GPT-5-nano vs Vertex AI
        var outputPrice = aiModel == "azure" ? 0.40 : 0.40; // GPT-5-nano vs Vertex AI

        var inputTokens = (int)Math.Ceiling(inputText.Length / 4.0); // Rough estimate: 1 token ~ 4 characters
        var outputTokens = (int)Math.Ceiling(outputText.Length / 4.0); // Rough estimate: 1 token ~ 4 characters
        var inputCost = (inputTokens / 1_000_000.0) * inputPrice; // $0.25 vs $0.10 per million tokens
        var outputCost = (outputTokens / 1_000_000.0) * outputPrice; // $2.00 vs $0.40 per million tokens

        return new RequestCost
        {
            InputTokens = inputTokens,
            OutputTokens = outputTokens,
            InputCost = (decimal)inputCost,
            OutputCost = (decimal)outputCost,
            ProcessingTimeInMiliseconds = elapsedMs
        };
    }

    private void AddPastMessagesToHistory(ChatHistory history, IEnumerable<ChatMessage> pastMessages)
    {
        foreach (var msg in pastMessages)
        {
            if (msg.Role == "user")
            {
                history.AddUserMessage(msg.Content);
            }
            else
            {
                history.AddAssistantMessage(msg.Content);
            }
        }
    }

    private static readonly Dictionary<string, string[]> CitiesByCounty =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Cluj"] = new[] { "Cluj-Napoca", "Turda", "Dej" },
            ["Timiș"] = new[] { "Timișoara", "Lugoj", "Sânnicolau Mare" },
            ["Bihor"] = new[] { "Oradea", "Salonta", "Beiuș" }
        };

    private static IEnumerable<string> PickRandomDistinct(IList<string> items, int count)
    {
        if (items.Count <= count) return items;

        var result = new List<string>(count);
        var picked = new HashSet<int>();
        while (result.Count < count && picked.Count < items.Count)
        {
            var idx = RandomNumberGenerator.GetInt32(items.Count);
            if (picked.Add(idx))
                result.Add(items[idx]);
        }
        return result;
    }

    private static bool TryGetCities(string county, out string[] cities)
    {
        // unifică „Timis” -> „Timiș”
        var key = CitiesByCounty.ContainsKey(county) ? county : county switch
        {
            "Timis" => "Timiș",
            _ => county
        };
        return CitiesByCounty.TryGetValue(key, out cities!);
    }

    private static string CanonicalCounty(string county) =>
        county.Equals("Timis", StringComparison.OrdinalIgnoreCase) ? "Timiș" : county;

    private static string FormatPair(string county, string city) => $"{county} — {city}";

    private static void AddOnce(List<string> list, HashSet<string> seen, string value)
    {
        if (seen.Add(value)) list.Add(value);
    }

    private List<string>? GenerateSmartSuggestions(UserPreferences? preferences)
    {
        var suggestions = new List<string>();

        if (preferences is null || preferences.IsEmpty() || string.IsNullOrWhiteSpace(preferences!.TransactionType))
        {
            suggestions.Add("Vreau să cumpăr");
            suggestions.Add("Vreau să închiriez");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.PropertyType))
        {
            suggestions.Add("Garsoniera");
            suggestions.Add("Apartament");
            suggestions.Add("Casa");
            return suggestions;
        }

        if (!preferences.RoomNumbers.HasValue)
        {
            suggestions.Add("1 cameră");
            suggestions.Add("2 camere");
            suggestions.Add("3 camere");
            suggestions.Add("4 camere sau mai mult");
            return suggestions;
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(preferences.County))
        {
            var allPairs = CitiesByCounty
            .Select(kv => (County: CanonicalCounty(kv.Key), Cities: kv.Value))
            .GroupBy(x => x.County, StringComparer.OrdinalIgnoreCase)
            .Select(g => (County: g.Key, Cities: g.First().Cities)) // dedup „Timiș/Timis”
            .SelectMany(x => x.Cities.Select(cityName => FormatPair(x.County, cityName)))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

            foreach (var pick in PickRandomDistinct(allPairs, 3))
                AddOnce(suggestions, seen, pick);

            return suggestions;
        }

        if (!string.IsNullOrWhiteSpace(preferences.County) && string.IsNullOrWhiteSpace(preferences.City))
        {
            if (TryGetCities(preferences.County, out var cities))
            {
                foreach (var c in cities.Take(3))
                    AddOnce(suggestions, seen, FormatPair(preferences.County, c));
            }
            else
            {
                AddOnce(suggestions, seen, FormatPair(preferences.County, CitiesByCounty[preferences.County][0]));
            }

            return suggestions;
        }

        if (!preferences.MaxPrice.HasValue)
        {
            suggestions.Add("Sub 50.000 EUR");
            suggestions.Add("Până în 100.000 EUR");
            suggestions.Add("Până în 200.000 EUR");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.TextFilter))
        {
            suggestions.Add("Lângă scoala gimnaziala");
            suggestions.Add("Zonă o clinica medicală");
            suggestions.Add("Lângă un parc");
            return suggestions;
        }

        return null;
    }

}
