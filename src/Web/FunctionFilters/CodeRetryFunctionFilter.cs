using DslCopilot.Web.Services;
using DslCopilot.Web.Validators;
using Markdig.Helpers;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DslCopilot.Web.FunctionFilters
{
#pragma warning disable SKEXP0001 // Experimental API
  public class CodeRetryFunctionFilter : IFunctionFilter
  {

    private ChatSessionService _chatSessionService;
    private Dictionary<string, int> _numRetries = new();
    private Kernel _kernel;

    private const int MAX_RETRIES = 3;

    public CodeRetryFunctionFilter(ChatSessionService chatSessionService, Kernel kernel)
    {
      _kernel = kernel;
      _chatSessionService = chatSessionService;
    }

    public void OnFunctionInvoked(FunctionInvokedContext context)
    {
      if (context.Function.Name != "CodeGen")
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

      var chatSession = _chatSessionService.GetChatSession(chatSessionId);

      var code = context.Result.GetValue<string>();
      if (string.IsNullOrEmpty(code))
      {
        return;
      }

      var result = CSharpCodeValidator.ValidateCode(code);

      if (!result.IsValid)
      {
        if (!_numRetries.ContainsKey(operationId))
        {
          _numRetries.Add(operationId, 0);
        }
        
        _numRetries[operationId] += 1;
        if (_numRetries[operationId] < MAX_RETRIES)
        {
          chatSession.AddUserMessage("Unfortunately, this code contains the following errors:");
          foreach (var error in result.Errors)
          {
            chatSession.AddUserMessage(error);
          }
          chatSession.AddUserMessage("Please correct the errors and try again.");

          context.Arguments.Remove("history");
          context.Arguments.Add("history", chatSession);
          // re-invoke the function
          var nextResult = context.Function.InvokeAsync(_kernel, context.Arguments).GetAwaiter().GetResult();

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

    public void OnFunctionInvoking(FunctionInvokingContext context)
    {
      return;
    }
  }
#pragma warning restore SKEXP0001 // Experimental API
}
