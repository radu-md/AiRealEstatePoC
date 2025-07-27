using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public interface IQueryBuilderService
{
    string? BuildUrl(UserPreferences? prefs);
}
