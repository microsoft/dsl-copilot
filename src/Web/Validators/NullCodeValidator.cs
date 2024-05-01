namespace DslCopilot.Web.Validators
{
  public class NullCodeValidator : ICodeValidator
  {
    public string Name => nameof(NullCodeValidator);

    public CodeValidationResult ValidateCode(string input)
    {
      return new CodeValidationResult
      {
        IsValid = true,
        Errors = []
      };
    }
  }
}
