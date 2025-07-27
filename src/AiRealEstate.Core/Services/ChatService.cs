using AiRealEstate.Core.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiRealEstate.Core.Services;

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chat;
    private readonly IConversationStateService _state;
    private readonly IQueryBuilderService _queryBuilder;

    public ChatService(Kernel kernel, IConversationStateService state, IQueryBuilderService queryBuilder)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
        _state = state;
        _queryBuilder = queryBuilder;
    }

    public async Task<ChatResult> GetResponseAsync(string sessionId, string message)
    {
        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "user",
            Content = message
        });

        var history = new ChatHistory();
        var pastMessages = _state.GetHistory(sessionId);

        foreach (var msg in pastMessages)
        {
            if (msg.Role == "user")
                history.AddUserMessage(msg.Content);
            else
                history.AddAssistantMessage(msg.Content);
        }

        var result = await _chat.GetChatMessageContentAsync(history);

        if (result is null || string.IsNullOrWhiteSpace(result.Content))
        {
            //return "Imi pare rau, nu am putut genera un raspuns.";
            return new ChatResult
            {
                Response = "Imi pare rau, nu am putut genera un raspuns."
            };
        }

        _state.AddMessage(sessionId, new ChatMessage
        {
            Role = "assistant",
            Content = result.Content
        });

        var dummyPrefs = new UserPreferences
        {
            PropertyType = "apartamente",
            TransactionType = "de-inchiriat",
            County = "Cluj",
            City = "Cluj-Napoca",
            MaxPrice = 550,
            TextFilter = "modern"
        };

        return new ChatResult
        {
            Response = result.Content,
            Listings = new List<Listing> { 
             new Listing
                {
                    Title = "Apartament modern cu 2 camere in Cluj-Napoca",
                    Image = "https://s3.publi24.ro/vertical-ro-f646bd5a/top/20250522/1328/b9bf1e08f43580ea606fb30c9f902e0f.webp",
                    Price = 550,
                    Link = _queryBuilder.BuildUrl(dummyPrefs) ?? "https://www.romimo.ro",
                }
            },
            SuggestedQuestions = new List<string>
            {
                    "Ce tip de proprietate cauti?",
                    "Care este bugetul tau?",
                    "In ce zona te intereseaza sa cauti proprietati?",
                    "Cauti o proprietate pentru investitie sau pentru locuit?",
                    "Ai nevoie de informatii despre finantare sau credite ipotecare?"
            }
        };
    }
}
