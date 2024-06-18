using System.ComponentModel;
using System.Text.Json;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace DslCopilot.Core.Plugins;

using Models;

public class CodeExampleRetrievalPlugins(
    SearchClient searchClient,
    IFileProvider fileProvider,
    CodeExampleRetrievalPluginOptions options)
{
  [KernelFunction]
  [Description("Get local examples for a given language.")]
  [return: Description("A list of code block examples for a given language.")]
  public async Task<IEnumerable<CodeBlock>> GetLocalExamples(
      [Description("The language to get examples for.")] string language,
      CancellationToken cancellationToken)
  {
    var fileInfo = fileProvider.GetFileInfo($"{options.ExamplesPath}/{language}.yaml");
    if (!fileInfo.Exists)
    {
      throw new FileNotFoundException($"No local examples found for language {language}");
    }
    
    var examples = await fileInfo
      .ReadAllAsync(cancellationToken)
      .ConfigureAwait(false);

    var deserializer = new DeserializerBuilder()
      .WithNamingConvention(CamelCaseNamingConvention.Instance)
      .Build();
    var codeExamples = deserializer.Deserialize<LanguageExamples>(examples);
    return codeExamples.Prompts;
  }

  [KernelFunction]
  [Description("Get indexed examples for a given input and language.")]
  [return: Description("A list of code block examples for a given input and language.")]
  public async Task<IEnumerable<CodeBlock>> GetIndexedExamples(
      [Description("The user prompt.")] string userPrompt,
      [Description("The language to get examples for.")] string language,
      CancellationToken cancellationToken)
  {
    List<CodeBlock> examples = [];
    SearchResults<CodeExample> response = await searchClient.SearchAsync<CodeExample>(userPrompt, new SearchOptions
    {
      Filter = $"tags/any(t: t eq 'language:{language}')",
      SemanticSearch = new()
      {
        SemanticConfigurationName = options.SemanticConfigurationName,
        QueryCaption = new(QueryCaptionType.Extractive),
        QueryAnswer = new(QueryAnswerType.Extractive)
      },
      QueryType = SearchQueryType.Semantic
    }, cancellationToken);

    JsonSerializerOptions jsonOptions = new() { PropertyNameCaseInsensitive = true };
    await foreach (var result in response.GetResultsAsync())
    {
      var payloadObject = JsonSerializer.Deserialize<Payload>(result.Document.payload, jsonOptions);
      examples.Add(new(payloadObject!.Response, payloadObject.AdditionalDetails, payloadObject.Prompt));
    }

    return examples;
  }
}
