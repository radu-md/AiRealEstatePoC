namespace AiRealEstate.Core.Models;

public class UserPreferences
{
    public string? PropertyType { get; set; } 
    public string? TransactionType { get; set; }  
    public string? County { get; set; }
    public string? City { get; set; }
    public int? MaxPrice { get; set; }
    public string? TextFilter { get; set; }
}
