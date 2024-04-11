namespace DslCopilot.Web.Options;
public record AzureOpenAIOptions(
  string? ApiKey,
  string? Endpoint,
  string? EmbeddingDeploymentName,
  string? CompletionDeploymentName
);
