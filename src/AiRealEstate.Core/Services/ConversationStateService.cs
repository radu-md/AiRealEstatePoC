using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public class ConversationStateService : IConversationStateService
{
    private readonly Dictionary<string, List<ChatMessage>> _store = new();

    public List<ChatMessage> GetHistory(string sessionId)
    {
        if (!_store.ContainsKey(sessionId)) return new();

        return _store[sessionId];
    }

    public void AddMessage(string sessionId, ChatMessage message)
    {
        if (!_store.ContainsKey(sessionId))
        {
            _store[sessionId] = new List<ChatMessage>();
        }

        var cleaned = CleanMessage(message.Content);

        _store[sessionId].Add(new ChatMessage
        {
            Role = message.Role,
            Content = cleaned,
            UserPreferences = message.UserPreferences
        });
                
        if (_store[sessionId].Count > 10)
        {
            _store[sessionId] = _store[sessionId].TakeLast(10).ToList();
        }
    }

    public void Clear(string sessionId)
    {
        if (_store.ContainsKey(sessionId))
        {
            _store.Remove(sessionId);
        }
    }

    private string CleanMessage(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return String.Empty;
        }

        return input.Length > 200 ? input.Substring(0, 200).Trim() : input.Trim();
    }
}
