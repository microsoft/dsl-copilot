namespace DslCopilot.Web.Validators;
public interface ICodeValidator
{
  CodeValidationResult ValidateCode(string input);
  string Name { get; }
}
