using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace AiRealEstate.Core.Services;

public class ChatService : IChatService
{
    private readonly IChatCompletionService _chat;

    public ChatService(Kernel kernel)
    {
        _chat = kernel.GetRequiredService<IChatCompletionService>();
    }

    public async Task<string> AskAsync(string prompt)
    {
        var result = await _chat.GetChatMessageContentAsync(prompt);
        return result.Content ?? string.Empty;
    }
}
