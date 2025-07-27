namespace AiRealEstate.Core.Models;

public class UserPreferences
{
    public string? PropertyType { get; set; } 
    public string? TransactionType { get; set; }  
    public string? County { get; set; }
    public string? City { get; set; }
    public int? MaxPrice { get; set; }
    public string? TextFilter { get; set; }

    public override string ToString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(PropertyType))
            parts.Add($"tip proprietate: {PropertyType}");
        if (!string.IsNullOrWhiteSpace(TransactionType))
            parts.Add($"tranzacție: {TransactionType}");
        if (!string.IsNullOrWhiteSpace(County))
            parts.Add($"județ: {County}");
        if (!string.IsNullOrWhiteSpace(City))
            parts.Add($"oraș: {City}");
        if (MaxPrice.HasValue)
            parts.Add($"buget maxim: {MaxPrice.Value} EUR");
        if (!string.IsNullOrWhiteSpace(TextFilter))
            parts.Add($"preferințe: {TextFilter}");

        return string.Join(", ", parts);
    }
}
