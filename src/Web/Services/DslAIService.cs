using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace DslCopilot.Web.Services;
public class DslAIService(
  Kernel kernel,
  IKernelMemory memory,
  ChatSessionIdService chatSessionIdService,
  ChatSessionService chatSessionService)
{
  public async Task<string> AskAI(string userMessage, string antlrDef, string language, CancellationToken cancellationToken)
  {
    var chatSessionId = chatSessionIdService.GetChatSessionId();
    var chatHistory = chatSessionService.GetChatSession(chatSessionId);

    var operationId = Guid.NewGuid().ToString();

    if (chatHistory.Count == 0)
    {
      chatHistory.AddSystemMessage($"You are an assistant for generating code that conforms to a given grammar.");
    }

    chatHistory.AddUserMessage(userMessage);

    var fewShotExamples = await GetFewShotExamples(userMessage, cancellationToken);
    var result = await kernel.InvokeAsync("yaml_plugins", "generateCode", new()
      {
        { "input", userMessage },
        { "history", string.Join(Environment.NewLine, chatHistory) },
        { "grammar", antlrDef },
        { "fewShotExamples", fewShotExamples },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId },
        { "language", language }
      }, cancellationToken);

    return result.GetValue<string>() ?? string.Empty;
  }

  private async Task<string> GetFewShotExamples(string input, CancellationToken cancellationToken)
  {
    IEnumerable<string?> examples = await File.ReadAllLinesAsync("examples/csharp.md", cancellationToken);
    var memories = await memory.SearchAsync(input, limit: 3, cancellationToken: cancellationToken);
    var results = examples.Concat([memories.ToString()]).Where(x => x != null).Cast<string>();
    return string.Join(Environment.NewLine, results);
  }
}
