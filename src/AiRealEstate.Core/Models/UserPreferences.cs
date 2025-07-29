namespace AiRealEstate.Core.Models;

public class UserPreferences
{
    public string? PropertyType { get; set; } 
    public string? TransactionType { get; set; }  
    public string? County { get; set; }
    public string? City { get; set; }
    public int? MaxPrice { get; set; }
    public string? TextFilter { get; set; }

    public bool IsEmpty()
    {
        return String.IsNullOrWhiteSpace(TransactionType) &&
               String.IsNullOrWhiteSpace(PropertyType) &&
               String.IsNullOrWhiteSpace(City) &&
               String.IsNullOrWhiteSpace(County) &&
               MaxPrice == null &&
               String.IsNullOrWhiteSpace(TextFilter);
    }

    public void Merge(UserPreferences? other)
    {
        if (other == null) return;

        if ((string.IsNullOrWhiteSpace(TransactionType) && !string.IsNullOrWhiteSpace(other.TransactionType)) || 
            (!string.IsNullOrWhiteSpace(TransactionType) && !string.IsNullOrWhiteSpace(other.TransactionType) && TransactionType != other.TransactionType))
            TransactionType = other.TransactionType;

        if ((string.IsNullOrWhiteSpace(PropertyType) && !string.IsNullOrWhiteSpace(other.PropertyType)) ||
            (!string.IsNullOrWhiteSpace(PropertyType) && !string.IsNullOrWhiteSpace(other.PropertyType) && PropertyType != other.PropertyType))
            PropertyType = other.PropertyType;

        if ((string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(other.City)) ||
            (!string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(other.City) && City != other.City))
            City = other.City;

        if ((string.IsNullOrWhiteSpace(County) && !string.IsNullOrWhiteSpace(other.County)) ||
            (!string.IsNullOrWhiteSpace(County) && !string.IsNullOrWhiteSpace(other.County) && County != other.County))
            County = other.County;

        if ((!MaxPrice.HasValue && other.MaxPrice.HasValue) ||
            (MaxPrice.HasValue && other.MaxPrice.HasValue && MaxPrice != other.MaxPrice))
            MaxPrice = other.MaxPrice;

        if ((string.IsNullOrWhiteSpace(TextFilter) && !string.IsNullOrWhiteSpace(other.TextFilter)) ||
            (!string.IsNullOrWhiteSpace(TextFilter) && !string.IsNullOrWhiteSpace(other.TextFilter) && TextFilter != other.TextFilter))
            TextFilter = other.TextFilter;
    }

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
            parts.Add($"filtru text: {TextFilter}");

        return string.Join(", ", parts);
    }
}
