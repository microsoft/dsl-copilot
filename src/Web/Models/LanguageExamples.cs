namespace DslCopilot.Web.Models
{
  public record LanguageExamples
  {
    public string? Language { get; set; }

    public List<CodeBlock> Prompts { get; set; } = [];
  }
}
