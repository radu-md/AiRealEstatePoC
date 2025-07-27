
namespace AiRealEstate.Core.Services;

public interface IChatService
{
    Task<string> GetResponseAsync(string prompt);
}