namespace DslCopilot.Web.Models
{
  public record CodeBlock
  {
    public string Prompt { get; set; }

    public string AdditionalDetails { get; set; }

    public string Response { get; set; }
  }
}
