using HtmlAgilityPack;
using AiRealEstate.Core.Models;
using AiRealEstate.Core.Services;

namespace AiRealEstate.Infrastructure.Services;

public class ListingScraperService : IListingScraperService
{
    public async Task<Listing?> ExtractListingFromUrlAsync(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Extract title
            var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(@itemprop, 'name')]");
            var title = titleNode?.InnerText.Trim() ?? "";

            // Extract price
            var priceNode = doc.DocumentNode.SelectSingleNode("//span[contains(@itemprop, 'price')]");
            var priceText = priceNode?.InnerText.Trim() ?? "0";
            int.TryParse(priceText, out int price);

            // Extract image
            var imageNode = doc.DocumentNode.SelectSingleNode("//img[contains(@itemprop, 'image')]");
            var image = imageNode?.GetAttributeValue("src", "") ?? "";

            // Link is the input url
            var link = url;

            return new Listing
            {
                Title = title,
                Price = price,
                Image = image,
                Link = link
            };
        }
        catch 
        {
            return null;
        }
    }
}
