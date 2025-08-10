using AiRealEstate.Core.Models;
using AiRealEstate.Core.Skills;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiRealEstate.Core.Services;

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chat;
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
        IListingScraperService listingScraperService)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _state = state;
        _queryBuilder = queryBuilder;
        _extractUserPreferencesSkill = extractUserPreferencesSkill;
        _userPreferencesStateService = userPreferencesStateService;
        _listingScraperService = listingScraperService;
    }

    public async Task<ChatResult> GetResponseAsync(string sessionId, string? newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
        {
            return new ChatResult { Response = "Te rog să introduci un mesaj." };
        }

        var pastMessages = _state.GetHistory(sessionId);
        var newUserPreferences = _userPreferencesStateService.UpdatePreferences(sessionId, await _extractUserPreferencesSkill.ExtractAsync(newMessage));
        var suggestedQuestions = GenerateSmartSuggestions(newUserPreferences);

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
        if (suggestedQuestions is not null && suggestedQuestions.Any())
        {
            history.AddSystemMessage($"Întrebări sugerate: {String.Join(" ", suggestedQuestions)}");
        }

        AddPastMessagesToHistory(history, pastMessages);

        history.AddUserMessage(newMessage);

        var assistantReply = await _chat.GetChatMessageContentAsync(history);
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

        if (string.IsNullOrWhiteSpace(adUrl))
        {
            return new ChatResult
            {
                Response = assistantReply.Content,
                SuggestedQuestions = suggestedQuestions
            };
        }

        var listing = await _listingScraperService.ExtractListingFromUrlAsync(adUrl);
        var listings = listing is not null ? new List<Listing> { listing } : null;

        return new ChatResult
        {
            Response = assistantReply.Content,
            Listings = listings,
            SuggestedQuestions = suggestedQuestions
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

    private List<string>? GenerateSmartSuggestions(UserPreferences? preferences)
    {
        var suggestions = new List<string>();

        bool allNullOrEmpty = preferences == null ||
            (string.IsNullOrWhiteSpace(preferences.TransactionType) &&
             string.IsNullOrWhiteSpace(preferences.PropertyType) &&
             string.IsNullOrWhiteSpace(preferences.City) &&
             !preferences.MaxPrice.HasValue &&
             string.IsNullOrWhiteSpace(preferences.TextFilter));

        if (allNullOrEmpty)
        {
            suggestions.Add("Doresti să cumperi sau să închiriezi?");
            suggestions.Add("Caut o locuință de vânzare");
            suggestions.Add("Caut o locuință de închiriat");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences!.TransactionType))
        {
            suggestions.Add("Ce anume vrei?");
            suggestions.Add("Vreau să cumpăr");
            suggestions.Add("Vreau să închiriez");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.PropertyType))
        {
            suggestions.Add("Ce tip de locuință preferi?");
            suggestions.Add("Garsoniera");
            suggestions.Add("Apartament");
            suggestions.Add("Casa");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.County))
        {
            suggestions.Add("În ce județ dorești să cauți?");
            suggestions.Add("Cluj");
            suggestions.Add("Timiș");
            suggestions.Add("Bihor");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.City))
        {
            suggestions.Add("În ce localitate dorești să cauți?");
            switch (preferences.County?.Trim().ToLower())
            {
                case "cluj":
                    suggestions.Add("Cluj-Napoca");
                    suggestions.Add("Turda");
                    suggestions.Add("Dej");
                    break;
                case "timiș":
                case "timis":
                    suggestions.Add("Timișoara");
                    suggestions.Add("Lugoj");
                    suggestions.Add("Sânnicolau Mare");
                    break;
                case "bihor":
                    suggestions.Add("Oradea");
                    suggestions.Add("Salonta");
                    suggestions.Add("Beiuș");
                    break;
                default:
                    suggestions.Add("Localitate principală din județ");
                    break;
            }
            return suggestions;
        }

        if (!preferences.MaxPrice.HasValue)
        {
            suggestions.Add("Care este bugetul tău maxim?");
            suggestions.Add("Sub 50.000 EUR");
            suggestions.Add("Până în 100.000 EUR");
            suggestions.Add("Până în 200.000 EUR");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.TextFilter))
        {
            suggestions.Add("Ai vreo preferință legată de zonă sau facilități?");
            suggestions.Add("Lângă scoala gimnaziala");
            suggestions.Add("Zonă liniștită");
            suggestions.Add("Lângă un parc");
            return suggestions;
        }

        return null;
    }

}
