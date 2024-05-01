using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static System.Environment;

namespace DslCopilot.Web.Validators;
public class CSharpCodeValidator : ICodeValidator
{
  public string Name => nameof(CSharpCodeValidator);

  public CodeValidationResult ValidateCode(string input)
  {
    CodeValidationResult? result;
    try
    {
      result = ValidateCSharpCode(input);
    }
    catch (Exception ex)
    {
      result = new CodeValidationResult
      {
        IsValid = false,
        Errors = [ex.Message]
      };
    }
    return result;
  }

  private static CodeValidationResult ValidateCSharpCode(string code)
  {
    CodeValidationResult result = new();
    code = code.ReplaceLineEndings(NewLine);
    var syntaxTree = CSharpSyntaxTree.ParseText(code);
    var thisAssembly = typeof(CSharpCodeValidator).Assembly;
    var referencedAssemblies = thisAssembly.GetReferencedAssemblies()
        .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location));
    var compilation = CSharpCompilation.Create("ValidationCompilation")
        .AddReferences(referencedAssemblies)
        .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
        .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
        .AddSyntaxTrees(syntaxTree);

    var errors = compilation.GetDiagnostics()
        .Where(x => x.Severity == DiagnosticSeverity.Error)
        .Select(x => x.GetMessage());
    if (errors.Any())
    {
      result.IsValid = false;
      result.Errors = errors.ToList();
      return result;
    }
    result.IsValid = true;
    return result;
  }
}
