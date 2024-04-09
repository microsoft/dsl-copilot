namespace DslCopilot.Web.Options;
public class AzureOpenAIOptions
{
  public string? ApiKey { get; set; }
  public string? Endpoint { get; set; }
  public string? EmbeddingDeploymentName { get; set; }
  public string? CompletionDeploymentName { get; set; }
}
