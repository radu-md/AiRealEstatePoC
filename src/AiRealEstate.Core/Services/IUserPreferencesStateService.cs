using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services
{
    public interface IUserPreferencesStateService
    {
        UserPreferences GetUserPreferences(string sessionId);
        UserPreferences UpdatePreferences(string sessionId, UserPreferences newPrefs);
        void ResetPreferences(string sessionId);
    }
}
