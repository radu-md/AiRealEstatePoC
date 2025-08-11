using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Skills
{
    public interface IExtractUserPreferencesSkill
    {
        Task<UserPreferences> ExtractAsync(string aiModel,string userMessage);
    }
}