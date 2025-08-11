using System.Text;

namespace AiRealEstate.Core.Infrastructure;

public class VertexAI
{
    public required string ProjectId { get; set; }
    public required string Location { get; set; }
    public required string Model { get; set; }
    public required string ServiceAccountBase64 { get; set; }

    public string GetServiceAccountJson() =>
        Encoding.UTF8.GetString(Convert.FromBase64String(ServiceAccountBase64));
}
