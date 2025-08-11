using AiRealEstate.Core.Models;
using System.Collections.Concurrent;

namespace AiRealEstate.Core.Services;

public class ConversationStateService : IConversationStateService
{
    private readonly ConcurrentDictionary<string, List<ChatMessage>> _store = new();

    public List<ChatMessage> GetHistory(string sessionId)
    {
        if (!_store.ContainsKey(sessionId))
        {
            _store[sessionId] = new List<ChatMessage>();
        }

        return _store[sessionId];
    }

    public void AddMessage(string sessionId, ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(message.Content))
        {
            throw new ArgumentException("Message content cannot be null or empty.", nameof(message));
        }

        var cleaned = CleanMessage(message.Content);

        var newMessage = new ChatMessage
        {
            Role = message.Role,
            Content = cleaned
        };

        var list = _store.GetOrAdd(sessionId, _ => new List<ChatMessage>());
        lock (list)
        {
            list.Add(newMessage);
        }
                        
        if (_store[sessionId].Count > 10)
        {
            _store[sessionId] = _store[sessionId].TakeLast(10).ToList();
        }
    }

    public void RemoveAllMessages(string sessionId)
    {
        if (_store.ContainsKey(sessionId))
        {
            lock (_store[sessionId])
            {
                _store[sessionId].Clear();
            }
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
