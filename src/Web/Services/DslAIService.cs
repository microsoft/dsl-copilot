using DslCopilot.Web.Options;
using Markdig.Helpers;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;

namespace DslCopilot.Web.Services
{
  public class DslAIService
  {

    Kernel _kernel;

    ChatSessionIdService _chatSessionIdService;
    ChatSessionService _chatSessionService;
    IChatCompletionService _chatCompletionService;
    OpenAIPromptExecutionSettings _executionSettings;

    public DslAIService(
      Kernel kernel, 
      ChatSessionIdService chatSessionIdService,
      ChatSessionService chatSessionService)
    {
      _kernel = kernel;
      _chatSessionIdService = chatSessionIdService;
      _chatSessionService = chatSessionService;

      _chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

      _executionSettings = new OpenAIPromptExecutionSettings()
      {
        ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
      };
    }

    public async Task<string> AskAI(string userMessage, string antlrDef, CancellationToken cancellationToken)
    {
      var chatSessionId = _chatSessionIdService.GetChatSessionId();
      var chatHistory = _chatSessionService.GetChatSession(chatSessionId);

      var operationId = Guid.NewGuid().ToString();

      if (chatHistory.Count == 0)
      {
        chatHistory.AddSystemMessage($"You are an assistant for generating code that conforms to a given ANTLR grammar.  You only respond with code, and you only respond with code that conforms to this grammar: {antlrDef}");
      }

      chatHistory.AddUserMessage(userMessage);
      _kernel.Data["chatSessionId"] = chatSessionId;
      _kernel.Data["operationId"] = operationId;

      //var result = _chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, _executionSettings);

      var result = await _kernel.InvokeAsync("plugins", "CodeGen", new()
      {
        { "input", userMessage },
        { "history", string.Join(Environment.NewLine, chatHistory) },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId }
      }, cancellationToken);

      return result.GetValue<string>() ?? string.Empty;
    }
  }
}
