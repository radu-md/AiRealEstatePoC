using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public interface IQueryBuilderService
{
    Task<string?> BuildUrlAsync(UserPreferences? prefs);
}
