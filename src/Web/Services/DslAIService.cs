using Microsoft.SemanticKernel;

namespace DslCopilot.Web.Services;
public class DslAIService(
  Kernel kernel,
  ChatSessionIdService chatSessionIdService,
  ChatSessionService chatSessionService)
{
  public async Task<string> AskAI(string userMessage, string antlrDef, CancellationToken cancellationToken)
  {
    var chatSessionId = chatSessionIdService.GetChatSessionId();
    var chatHistory = chatSessionService.GetChatSession(chatSessionId);

    var operationId = Guid.NewGuid().ToString();

    if (chatHistory.Count == 0)
    {
      chatHistory.AddSystemMessage($"You are an assistant for generating code that conforms to a given ANTLR grammar.  You only respond with code, and you only respond with code that conforms to this grammar: {antlrDef}");
    }

    chatHistory.AddUserMessage(userMessage);
    kernel.Data["chatSessionId"] = chatSessionId;
    kernel.Data["operationId"] = operationId;

    var result = await kernel.InvokeAsync("yaml_plugins", "generateCode", new()
      {
        { "input", userMessage },
        { "history", string.Join(Environment.NewLine, chatHistory) },
        { "grammar", antlrDef },
        { "fewShotExamples", await File.ReadAllLinesAsync("examples/csharp.md", cancellationToken) },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId }
      }, cancellationToken);

    return result.GetValue<string>() ?? string.Empty;
  }
}
