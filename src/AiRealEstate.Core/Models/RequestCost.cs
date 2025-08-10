namespace AiRealEstate.Core.Models
{
    public class RequestCost
    {
        public int InputTokens { get; internal set; } = 0;
        public int OutputTokens { get; internal set; } = 0;
        public decimal InputCost { get; internal set; } = 0;
        public decimal OutputCost { get; internal set; } = 0;
        public decimal TotalCost => InputCost + OutputCost;
        public long ProcessingTimeInMiliseconds { get; set; }
    }
}