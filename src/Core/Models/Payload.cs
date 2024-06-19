namespace DslCopilot.Core.Models
{
  public record Payload(
    string? AdditionalDetails,
    string? Prompt,
    string? Response
  );
}
