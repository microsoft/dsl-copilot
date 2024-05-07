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

    var fewShotExamples = await GetFewShotExamples(userMessage, language, cancellationToken).ConfigureAwait(false);
    var result = await kernel.InvokeAsync("yaml_plugins", "generateCode", new()
      {
        { "input", userMessage },
        { "history", string.Join(Environment.NewLine, chatHistory) },
        { "language", language },
        { "grammar", antlrDef },
        { "fewShotExamples", fewShotExamples },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId },
      }, cancellationToken).ConfigureAwait(false);

    return result.GetValue<string>() ?? string.Empty;
  }

  private async Task<string> GetFewShotExamples(string input, string language, CancellationToken cancellationToken)
  {
    IEnumerable<string?> examples = [];
    if (File.Exists($"examples/{language}.md"))
    {
      examples = await File
        .ReadAllLinesAsync($"examples/{language}.md", cancellationToken)
        .ConfigureAwait(false);
    }

    var languageFilter = MemoryFilters.ByTag("language", language);

    var memories = await memory
      .SearchAsync(query: input, index: "code-index", limit: 3, filter: languageFilter, cancellationToken: cancellationToken)
      .ConfigureAwait(false);

    var memoryResults = memories.Results
      .SelectMany(memory => memory.Partitions)
      .Select(partition => partition.Text);

    var results = examples.Concat(memoryResults);
    return string.Join(Environment.NewLine, results);
  }
}
