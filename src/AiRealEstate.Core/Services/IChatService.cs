
using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public interface IChatService
{
    Task<ChatResult> GetResponseAsync(string sessionId, string newMessage);
}