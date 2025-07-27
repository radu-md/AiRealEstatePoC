using AiRealEstate.Core.Models;

namespace AiRealEstate.Core.Services;

public interface IListingScraperService
{
    Task<Listing?> ExtractListingFromUrlAsync(string url);
}
