
namespace AiRealEstate.Core.Services;

public interface IChatService
{
    Task<string> AskAsync(string prompt);
}