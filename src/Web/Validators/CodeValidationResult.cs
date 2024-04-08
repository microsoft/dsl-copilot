namespace DslCopilot.Web.Validators
{
  public class CodeValidationResult
  {
    public bool IsValid { get; set; } = false;
    public List<string> Errors { get; set; } = new();
  }
}
