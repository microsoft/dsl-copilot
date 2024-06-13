namespace DslCopilot.Core.Models;

public record CodeBlock(
  string? Prompt,
  string? AdditionalDetails,
  string? Response);
