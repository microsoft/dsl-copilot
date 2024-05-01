namespace DslCopilot.Web.Validators
{
  public static class CodeValidatorFactory
  {
    public static ICodeValidator GetValidator(string language)
    {
      switch (language)
      {
        case "csharp":
          return new CSharpCodeValidator();
        default:
         return new NullCodeValidator();
      }
    }
  }
}
