namespace AiRealEstate.Core.Infrastructure;

public class AzureOpenAI
{
    public required string DeploymentName { get; set; }
    public required string Endpoint { get; set; }
    public required string ApiKey { get; set; }
}
