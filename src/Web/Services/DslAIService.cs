using DslCopilot.Web.Models;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
    var fewShotExamples = await GetFewShotExamples(userMessage, language, cancellationToken).ConfigureAwait(false);
    var indexedExamples = await GetIndexedExamples(userMessage, language, cancellationToken).ConfigureAwait(false);
    var result = await kernel.InvokeAsync("yaml_plugins", "generateCode", new()
      {
        { "input", userMessage },
        { "history", chatHistory.Select(c => new { role = c.Role.Label, content = c.Content }) },
        { "language", language },
        { "grammar", antlrDef },
        { "fewShotExamples", fewShotExamples },
        { "indexedExamples", indexedExamples },
        { "chatSessionId", chatSessionId },
        { "operationId", operationId },
      }, cancellationToken).ConfigureAwait(false);

    var response = result.GetValue<string>() ?? string.Empty;
    chatHistory.AddUserMessage(userMessage);
    chatHistory.AddAssistantMessage(response);

    return response;
  }

  private async Task<List<CodeBlock>> GetFewShotExamples(string input, string language, CancellationToken cancellationToken)
  {
    var fewShotExamples = new List<CodeBlock>();

    if (File.Exists($"examples/{language}.yaml"))
    {
      var examples = await File
        .ReadAllTextAsync($"examples/{language}.yaml", cancellationToken)
        .ConfigureAwait(false);

      var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
      var codeExamples = deserializer.Deserialize<LanguageExamples>(examples);

      if (codeExamples.Prompts is not null && codeExamples.Prompts.Count > 0)
      {
        fewShotExamples.AddRange(codeExamples.Prompts);
      }
    }

    return fewShotExamples;
  }

  private async Task<List<CodeBlock>> GetIndexedExamples(string input, string language, CancellationToken cancellationToken)
  {
    var indexedExamples = new List<CodeBlock>();
    var languageFilter = MemoryFilters.ByTag("language", language);
    var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    var memories = await memory
      .SearchAsync(query: input, index: "code-index", limit: 3, filter: languageFilter, cancellationToken: cancellationToken)
      .ConfigureAwait(false);

    var memoryResults = memories.Results
      .SelectMany(memory => memory.Partitions)
      .Select(partition => partition.Text);

    foreach(string memoryResult in memoryResults)
    {
      var codeBlock = deserializer.Deserialize<CodeBlock>(memoryResult);
      indexedExamples.Add(codeBlock);
    }

    return indexedExamples;
  }
}
