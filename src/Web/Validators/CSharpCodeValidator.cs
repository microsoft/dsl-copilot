using System.ComponentModel;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using static System.Environment;

namespace DslCopilot.Web.Validators
{

  public static class CSharpCodeValidator
  {
    public static CodeValidationResult ValidateCode(string input)
    {
      CodeValidationResult? result = null;
      try
      {
        result = ValidateCSharpCode(input);
      }
      catch (Exception ex)
      {
        result = new CodeValidationResult();
        result.IsValid = false;
        result.Errors.Add(ex.Message);
      }

      return result;
    }

    private static CodeValidationResult ValidateCSharpCode(string code)
    {
      CodeValidationResult result = new CodeValidationResult();
      code = code.ReplaceLineEndings(NewLine);
      //ConsoleAnnotator.WriteLine($"code_validation:{NewLine}{code}{NewLine}", ConsoleColor.DarkBlue);
      SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code);
      var thisAssembly = typeof(CSharpCodeValidator).Assembly;
      var referencedAssemblies = thisAssembly.GetReferencedAssemblies()
          .Select(x => MetadataReference.CreateFromFile(Assembly.Load(x).Location));
      CSharpCompilation compilation = CSharpCompilation.Create("ValidationCompilation")
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
}
