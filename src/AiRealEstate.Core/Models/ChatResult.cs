namespace AiRealEstate.Core.Models;

public class ChatResult
{
    public string Response { get; set; } = "";
    public List<Listing>? Listings { get; set; }
    public List<string>? SuggestedQuestions { get; set; }
}
