using System.ComponentModel;
using System.Text.Json;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Linq;

namespace DslCopilot.Core.Plugins;
using Models;

public record CodeExampleRetrievalPluginOptions(
    string ExamplesPath = "examples",
    string CodeIndex = "code-index",
    string SemanticConfigurationName = "code-index-config",
    int Limit = 3);

public record GrammarRetrievalPluginOptions(
    string GrammarPath = "grammar",
    string BlobContainerName = "languages");

public static class IFileExtensions
{
  public static async Task<string> ReadAllAsync(this IFileInfo file, CancellationToken cancellationToken)
  {
    await using var stream = file.CreateReadStream();
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync(cancellationToken)
        .ConfigureAwait(false);
  }
}

public class GrammarRetrievalPlugins(
  BlobServiceClient blobServiceClient,
  GrammarRetrievalPluginOptions options)
{
  [KernelFunction]
  [Description("Get the contents of a grammar file for given language.")]
  [return: Description("The contents of the grammar file.")]
  public async Task<string> GetGrammarContents(
      [Description("The language to get the grammar for.")] string language,
      CancellationToken cancellationToken)
  {
    var resultString = string.Empty;
    var client = blobServiceClient.GetBlobContainerClient(options.BlobContainerName);
    var blobs = client
        .GetBlobsByHierarchyAsync(
            prefix: language + "/",
            delimiter: "/",
            cancellationToken: cancellationToken)
        .AsPages()
        .SelectMany(page => page.Values.ToAsyncEnumerable())
        .Where(blobItem => !blobItem.IsPrefix && blobItem.Blob.Name.EndsWith(".g4"));

    await foreach (var blobItem in blobs)
    {
      var result = await client
          .GetBlobClient(blobItem.Blob.Name)
          .DownloadContentAsync(cancellationToken)
          .ConfigureAwait(false);

      resultString += result.Value.Content.ToString();
    }

    return resultString;
  }
}

public class CodeExampleRetrievalPlugins(
    SearchClient searchClient,
    CodeExampleRetrievalPluginOptions options)
{
  [KernelFunction]
  [Description("Get local examples for a given language.")]
  [return: Description("A list of code block examples for a given language.")]
  public async Task<IEnumerable<CodeBlock>> GetLocalExamples(
      [Description("The language to get examples for.")] string language,
      CancellationToken cancellationToken)
  {
    List<CodeBlock> fewShotExamples = [];
    if (File.Exists($"examples/{language}.yaml"))
    {
      var examples = await File
        .ReadAllTextAsync($"examples/{language}.yaml", cancellationToken)
        .ConfigureAwait(false);

      var deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();
      var codeExamples = deserializer.Deserialize<LanguageExamples>(examples);

      if (codeExamples.Prompts is not null && codeExamples.Prompts.Count() > 0)
      {
        fewShotExamples.AddRange(codeExamples.Prompts);
      }
    }

    return fewShotExamples;
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
