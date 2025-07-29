using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public interface IConversationStateService
{
    List<ChatMessage> GetHistory(string sessionId);
    void AddMessage(string sessionId, ChatMessage message);
}
