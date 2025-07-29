using AiRealEstate.Core.Models;
using System.Collections.Concurrent;

namespace AiRealEstate.Core.Services
{
    public class UserPreferencesStateService : IUserPreferencesStateService
    {
        private readonly ConcurrentDictionary<string, UserPreferences> _store = new();

        public UserPreferences GetUserPreferences(string sessionId)
        {
            if (!_store.ContainsKey(sessionId))
            {
                _store[sessionId] = new UserPreferences();
            }

            return _store[sessionId];
        }

        public UserPreferences UpdatePreferences(string sessionId, UserPreferences newPrefs)
        {
            var existing = GetUserPreferences(sessionId);
            existing.Merge(newPrefs);

            return existing;
        }

        public void ResetPreferences(string sessionId)
        {
            _store[sessionId] = new UserPreferences();
        }
    }
}
