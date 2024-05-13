using DslCopilot.Web.Services;
using DslCopilot.Web.Validators;
using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;
public class CodeRetryFunctionFilter(
  ChatSessionService chatSessionService,
  ConsoleService consoleService,
  Kernel kernel)
  : FunctionFilterBase("generateCode")
{
  private readonly Dictionary<string, int> _numRetries = [];

  private const int MAX_RETRIES = 3;

  protected override async Task OnFunctionInvokedAsync(FunctionInvokedContext context, CancellationToken token)
  {
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

    var language = context.Arguments["language"]?.ToString();
    if (string.IsNullOrEmpty(language))
    {
      return;
    }


    var code = context.Result.GetValue<string>();
    if (string.IsNullOrEmpty(code))
    {
      //TODO: Consider this an error?
      return;
    }

    var codeValidator = CodeValidatorFactory.GetValidator(language);
    consoleService.WriteToConsole(chatSessionId, $"Validating generated code with {codeValidator.Name}");

    var result = codeValidator.ValidateCode(code);
    if (!result.IsValid)
    {
      _numRetries.TryAdd(operationId, 0);
      _numRetries[operationId] += 1;
      consoleService.WriteToConsole(chatSessionId, $"Code generation had errors. Errors: {string.Join(Environment.NewLine, result.Errors)}");
      consoleService.WriteToConsole(chatSessionId, $"Retrying... (Attempt {_numRetries[operationId]})");

      if (_numRetries[operationId] < MAX_RETRIES)
      {
        var chatSession = chatSessionService.GetChatSession(chatSessionId);
        var originalPrompt = context.Arguments["input"]?.ToString();
        var errors = string.Join(Environment.NewLine, result.Errors);
        var newPrompt = string.Join(Environment.NewLine,
                                    code,
                                    "Unfortunately, the above code contains the following errors:",
                                    errors,
                                    "Please correct the errors and try again.");

        chatSession.AddUserMessage(originalPrompt);
        chatSession.AddAssistantMessage(code);

        context.Arguments["input"] = newPrompt;
        context.Arguments["history"] = chatSession;
        context.Arguments["errors"] = result.Errors;
        context.Arguments["badCode"] = code;

        // re-invoke the function
        var nextResult = await context.Function
          .InvokeAsync(kernel, context.Arguments, token)
          .ConfigureAwait(false);
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
}
