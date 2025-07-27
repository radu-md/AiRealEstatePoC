using AiRealEstate.Core.Models;
using AiRealEstate.Core.Services;

namespace AiRealEstate.Infrastructure.Services;

public class QueryBuilderService : IQueryBuilderService
{
    public string? BuildUrl(UserPreferences? prefs)
    {
        if (prefs is null || string.IsNullOrWhiteSpace(prefs.City) || string.IsNullOrWhiteSpace(prefs.County))
            return String.Empty;

        var propType = prefs.PropertyType?.ToLower() ?? "apartamente";
        var tranType = prefs.TransactionType?.ToLower() ?? "vanzare";

        var url = $"https://www.romimo.ro/{tranType}/{propType}/{prefs.County.ToLower()}/{prefs.City.ToLower()}/";

        if (prefs.MaxPrice.HasValue)
            url += $"?maxprice={prefs.MaxPrice.Value}";

        if (!string.IsNullOrWhiteSpace(prefs.TextFilter))
            url += (url.Contains("?") ? "&" : "?") + $"text={Uri.EscapeDataString(prefs.TextFilter)}";

        return url;
    }
}
