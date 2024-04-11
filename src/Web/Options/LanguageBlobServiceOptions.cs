namespace DslCopilot.Web.Options;
public record LanguageBlobServiceOptions(
  string? AccessKey,
  string? AccountName,
  string? ContainerName
);
