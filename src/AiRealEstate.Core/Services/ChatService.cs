using AiRealEstate.Core.Models;
using AiRealEstate.Core.Skills;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

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
        IListingScraperService listingScraperService)
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

        var chat = _kernel.GetRequiredService<IChatCompletionService>(aiModel);
        var pastMessages = _state.GetHistory(sessionId);
        var userPreferences = await _extractUserPreferencesSkill.ExtractAsync(aiModel, newMessage);        
        var newUserPreferences = _userPreferencesStateService.UpdatePreferences(sessionId, userPreferences);
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

        // convert history items to string concatenation for cost calculation

        string inputText = string.Join(" ", history
            .Select(m => m.Content));

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
            GPT-5-Mini
            romanian words: 1 token = 0.75 words
            $0.25 / 1M tokens
            $2.00 / 1M tokens

            Vertex AI
            romanian words: 1 token = 0.75 words
            $0.10 / 1M tokens
            $0.40 / 1M tokens
        */
        if (string.IsNullOrWhiteSpace(inputText) || string.IsNullOrWhiteSpace(outputText))
        {
            return new RequestCost();
        }

        var inputPrice = aiModel == "azure" ? 0.25 : 0.10; // GPT-5-Mini vs Vertex AI
        var outputPrice = aiModel == "azure" ? 2.00 : 0.40; // GPT-5-Mini vs Vertex AI

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
            //suggestions.Add("Doresti să cumperi sau să închiriezi?");
            suggestions.Add("Caut o locuință de vânzare");
            suggestions.Add("Caut o locuință de închiriat");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences!.TransactionType))
        {
            //suggestions.Add("Ce anume vrei?");
            suggestions.Add("Vreau să cumpăr");
            suggestions.Add("Vreau să închiriez");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.PropertyType))
        {
            //suggestions.Add("Ce tip de locuință preferi?");
            suggestions.Add("Garsoniera");
            suggestions.Add("Apartament");
            suggestions.Add("Casa");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.County))
        {
            //suggestions.Add("În ce județ dorești să cauți?");
            suggestions.Add("Cluj");
            suggestions.Add("Timiș");
            suggestions.Add("Bihor");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.City))
        {
            //suggestions.Add("În ce localitate dorești să cauți?");
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
            //suggestions.Add("Care este bugetul tău maxim?");
            suggestions.Add("Sub 50.000 EUR");
            suggestions.Add("Până în 100.000 EUR");
            suggestions.Add("Până în 200.000 EUR");
            return suggestions;
        }

        if (string.IsNullOrWhiteSpace(preferences.TextFilter))
        {
            //suggestions.Add("Ai vreo preferință legată de zonă sau facilități?");
            suggestions.Add("Lângă scoala gimnaziala");
            suggestions.Add("Zonă liniștită");
            suggestions.Add("Lângă un parc");
            return suggestions;
        }

        if (!preferences.IsEmpty())
        {
            suggestions.Add("Reseteaza filtrele de cautare");
            return suggestions;
        }

        return null;
    }

}
