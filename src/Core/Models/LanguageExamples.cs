namespace DslCopilot.Core.Models;

public record LanguageExamples(
  string? Language,
  IEnumerable<CodeBlock> Prompts);
