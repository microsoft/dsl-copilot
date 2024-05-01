using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;

namespace DslCopilot.Web.Services;
public class DslAIService(
  Kernel kernel,
  IKernelMemory memory,
  ChatSessionIdService chatSessionIdService,
  ChatSessionService chatSessionService)
{
  public async Task<string> AskAI(string userMessage, string language, string antlrDef, CancellationToken cancellationToken)
  {
    var chatSessionId = chatSessionIdService.GetChatSessionId();
    var chatHistory = chatSessionService.GetChatSession(chatSessionId);

    var operationId = Guid.NewGuid().ToString();

    if (chatHistory.Count == 0)
    {
      chatHistory.AddSystemMessage($"You are an assistant for generating code that conforms to a given grammar.");
    }

    chatHistory.AddUserMessage(userMessage);

    var fewShotExamples = await GetFewShotExamples(userMessage, cancellationToken).ConfigureAwait(false);
    var result = await kernel.InvokeAsync("yaml_plugins", "generateCode", new()
      {
        { "input", userMessage },
        { "history", string.Join(Environment.NewLine, chatHistory) },
        { "language", language },
        { "grammar", antlrDef },
        { "fewShotExamples", fewShotExamples },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId },
        { "language", language }
      }, cancellationToken).ConfigureAwait(false);

    return result.GetValue<string>() ?? string.Empty;
  }

  private async Task<string> GetFewShotExamples(string input, CancellationToken cancellationToken)
  {
    var examples = await File
      .ReadAllLinesAsync("examples/csharp.md", cancellationToken)
      .ConfigureAwait(false);
    var memories = await memory
      .SearchAsync(input, limit: 3, cancellationToken: cancellationToken)
      .ConfigureAwait(false);
    var results = examples.Concat([memories.ToString()]).Where(x => x != null).Cast<string>();
    return string.Join(Environment.NewLine, results);
  }
}
