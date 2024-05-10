namespace DslCopilot.Web.Options;
public class AzureOpenAIOptions
{
  public string? SearchApiKey { get; set; }
  public string? SearchEndpoint { get; set; }
  public string? ApiKey { get; set; }
  public string? Endpoint { get; set; }
  public string? EmbeddingDeploymentName { get; set; }
  public string? CompletionDeploymentName { get; set; }
  public bool? DebugPrompt { get; set; }
}
