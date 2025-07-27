using AiRealEstate.Core.Models;
using AiRealEstate.Core.Services;

namespace AiRealEstate.Infrastructure.Services;

public class QueryBuilderService : IQueryBuilderService
{
    public async Task<string?> BuildUrlAsync(UserPreferences? prefs)
    {
        if (prefs is null || string.IsNullOrWhiteSpace(prefs.County) || string.IsNullOrWhiteSpace(prefs.City))
            return string.Empty;

        // Build search query from preferences
        var terms = new List<string>();
        if (!string.IsNullOrWhiteSpace(prefs.PropertyType)) terms.Add(prefs.PropertyType);
        if (!string.IsNullOrWhiteSpace(prefs.TransactionType)) terms.Add(prefs.TransactionType);
        if (!string.IsNullOrWhiteSpace(prefs.County)) terms.Add(prefs.County);
        if (!string.IsNullOrWhiteSpace(prefs.City)) terms.Add(prefs.City);
        if (prefs.MaxPrice.HasValue) terms.Add($"maxprice {prefs.MaxPrice.Value}");
        if (!string.IsNullOrWhiteSpace(prefs.TextFilter)) terms.Add(prefs.TextFilter);

        var query = string.Join(" ", terms);
        var searchUrl = $"https://www.romimo.ro/imobiliare/?q={Uri.EscapeDataString(query)}";

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible; AiRealEstateBot/1.0)");
        var html = await httpClient.GetStringAsync(searchUrl);

        var doc = new HtmlAgilityPack.HtmlDocument();
        doc.LoadHtml(html);

        // Find first property link ending with .html
        var links = doc.DocumentNode.SelectNodes("//a[@href]");
        if (links != null)
        {
            foreach (var linkNode in links)
            {
                var href = linkNode.GetAttributeValue("href", "");
                if (href.StartsWith("https://www.romimo.ro") && href.EndsWith(".html"))
                {
                    return href;
                }
            }
        }
        return string.Empty;
    }
}
