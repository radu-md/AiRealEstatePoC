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
    private readonly IListingScraperService _listingScraperService;

    public ChatService(Kernel kernel, IConversationStateService state, IQueryBuilderService queryBuilder, 
        IExtractUserPreferencesSkill extractUserPreferencesSkill, IListingScraperService listingScraperService)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _state = state;
        _queryBuilder = queryBuilder;
        _extractUserPreferencesSkill = extractUserPreferencesSkill;
        _listingScraperService = listingScraperService;
    }

    public async Task<ChatResult> GetResponseAsync(string sessionId, string newMessage)
    {
        var history = new ChatHistory();
        history.AddSystemMessage("""
        Ești un asistent imobiliar inteligent. Răspunzi în limba română și ajuți utilizatorii să găsească proprietăți potrivite pe www.romimo.ro (foarte important sa fie specificat www.romimo.ro).

        Obiectivul tău este să înțelegi preferințele utilizatorului (ex: tipul locuinței, orașul, zona, prețul, facilități dorite) și să oferi răspunsuri clare și relevante.

        Dacă informațiile sunt incomplete, formulează întrebări prietenoase pentru a obține detalii suplimentare.

        Nu solicita niciodată date personale (nume, email, telefon). Fii politicos, concis și util.

        Blochează orice conținut inadecvat sau necorespunzător. Daca este asa in raspuns trebuie sa spui "Bine ai venit pe portalul www.romimo.ro, unde poți găsi cele mai bune oferte imobiliare din România. Cum te pot ajuta astăzi?".

        Nu repeta intrebarile si limiteaza-te la raspunsuri scurte si la obiect de maxim 200 de caractere.
        """);

        var pastMessages = _state.GetHistory(sessionId);

        string? lastPref = pastMessages
            .Where(m => m.Role == "user" && m.UserPreferences is not null)
            .Select(m => m.UserPreferences!.ToString())
            .LastOrDefault();

        foreach (var msg in pastMessages)
        {
            if (msg.Role == "user")
                history.AddUserMessage($"{ msg.Content} cu preferintele {msg.UserPreferences?.ToString() ?? String.Empty}");
            else
                history.AddAssistantMessage(msg.Content);
        }

        if (!string.IsNullOrWhiteSpace(newMessage))
        {
            history.AddUserMessage(newMessage);
        }
        else
        {
            return new ChatResult
            {
                Response = "Te rog să introduci un mesaj."
            };
        }

        var result = await _chat.GetChatMessageContentAsync(history);

        if (result is null || string.IsNullOrWhiteSpace(result.Content))
        {
            return new ChatResult
            {
                Response = "Imi pare rau, nu am putut genera un raspuns."
            };
        }

        var prefs = await _extractUserPreferencesSkill.ExtractAsync($"{newMessage} cu preferintele {lastPref ?? String.Empty}");

        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "user",
            Content = newMessage,
            UserPreferences = prefs
        });

        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "assistant",
            Content = result.Content
        });

        var url = await _queryBuilder.BuildUrlAsync(prefs);

        if (string.IsNullOrWhiteSpace(url))
        {
            return new ChatResult
            {
                Response = result.Content,
                SuggestedQuestions = GenerateSmartSuggestions(prefs)
            };
        }

        var listing = await _listingScraperService.ExtractListingFromUrlAsync(url);

        var listings = listing is not null ? new List<Listing> { listing } : null;

        return new ChatResult
        {
            Response = result.Content,
            Listings = listings,
            SuggestedQuestions = GenerateSmartSuggestions(prefs)
        };
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
            suggestions.Add("Cu ce te pot ajuta? Ai dori să cumperi sau să închiriezi?");
            suggestions.Add("Caut o locuință de vânzare");
            suggestions.Add("Caut o locuință de închiriat");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences!.TransactionType))
        {
            suggestions.Add("Ce tip de tranzacție cauți?");
            suggestions.Add("Vreau să cumpăr");
            suggestions.Add("Vreau să închiriez");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.PropertyType))
        {
            suggestions.Add("Ce tip de locuință preferi?");
            suggestions.Add("Garsoniere");
            suggestions.Add("Apartamente");
            suggestions.Add("Case");
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
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.TextFilter))
        {
            suggestions.Add("Ai vreo preferință legată de zonă sau facilități?");
            suggestions.Add("Lângă scoala gimnaziala");
            suggestions.Add("Zonă liniștită");
            suggestions.Add("Langa un parc");
            return suggestions;
        }

        return null;
    }

}
