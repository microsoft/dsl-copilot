using DslCopilot.Web.Services;
using DslCopilot.Web.Validators;
using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;
public class CodeRetryFunctionFilter(ChatSessionService chatSessionService, ConsoleService consoleService, Kernel kernel) : IFunctionFilter
{
  private readonly Dictionary<string, int> _numRetries = [];

    private const int MAX_RETRIES = 3;

  public async void OnFunctionInvoked(FunctionInvokedContext context)
  {
    if (context.Function.Name != "generateCode")
    {
      return;
    }

    var chatSessionId = context.Arguments["chatSessionId"]?.ToString();
    if (string.IsNullOrEmpty(chatSessionId))
    {
      return;
    }

    var operationId = context.Arguments["operationId"]?.ToString();
    if (string.IsNullOrEmpty(operationId))
    {
      return;
    }

    var code = context.Result.GetValue<string>();
    if (string.IsNullOrEmpty(code))
    {
      //TODO: Consider this an error?
      return;
    }

    var result = CSharpCodeValidator.ValidateCode(code);
    if (!result.IsValid)
    {
      _numRetries.TryAdd(operationId, 0);
      _numRetries[operationId] += 1;
      consoleService.WriteToConsole(chatSessionId, $"Code generation had errors. Errors: {string.Join(Environment.NewLine, result.Errors)}");
      consoleService.WriteToConsole(chatSessionId, $"Retrying... (Attempt {_numRetries[operationId]})");

      if (_numRetries[operationId] < MAX_RETRIES)
      {      
        var chatSession = chatSessionService.GetChatSession(chatSessionId);
        chatSession.AddUserMessage("Unfortunately, this code contains the following errors:");
        foreach (var error in result.Errors)
        {
          chatSession.AddUserMessage(error);
        }
        chatSession.AddUserMessage("Please correct the errors and try again.");
        context.Arguments["history"] = chatSession;
        // re-invoke the function
        var nextResult = await context.Function.InvokeAsync(kernel, context.Arguments);
        context.SetResultValue(nextResult.GetValue<string>());
      }
      else
      {
        var errorResponse = $"{code}{Environment.NewLine}I'm sorry, but the code I could generate contains the following errors.  You'll need to correct them before using this code.";
        foreach (var error in result.Errors)
        {
          errorResponse += $"{Environment.NewLine}{error}";
        }
        context.SetResultValue(errorResponse);
      }
    }
  }

  public void OnFunctionInvoking(FunctionInvokingContext context) { }
}
