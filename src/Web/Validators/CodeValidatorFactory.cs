namespace DslCopilot.Web.Validators;
public static class CodeValidatorFactory
{
  public static ICodeValidator GetValidator(string language) => language switch
  {
    "csharp" => new CSharpCodeValidator(),
    _ => new NullCodeValidator(),
  };
}
