namespace DslCopilot.Web.Options;

public record class LanguageBlobServiceOptions
{
  public string? AccessKey { get; set; }
  public string? AccountName { get; set; }
  public string? ContainerName { get; set; }
  public string? Endpoint { get; set; }
}
