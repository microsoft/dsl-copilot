using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Util.Internal;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Core.Models;
using DslCopilot.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.FileProviders;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core.Plugins;

public record CodeExampleRetrievalPluginOptions(
    string ExamplesPath = "examples",
    string CodeIndex = "code-index",
    string BlobContainerName = "languages",
    string SemanticConfigurationName = "code-index-config",
    int Limit = 3);

public record GrammarRetrievalPluginOptions(
    string GrammarPath = "grammar");

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
    GrammarRetrievalPluginOptions options)
{
    [KernelFunction]
    [Description("Get the contents of a grammar file for given language.")]
    [return: Description("The contents of the grammar file.")]
    public async Task<string> GetGrammarContents(
        [Description("The language to get the grammar for.")] string language,
        CancellationToken cancellationToken)
    {
    /*var languagePath = Path.Combine(options.GrammarPath, $"{language}.g4");
    var file = fileProvider.GetFileInfo(languagePath);
    return !file.Exists
        ? throw new FileNotFoundException(languagePath)
        : await file.ReadAllAsync(cancellationToken)
            .ConfigureAwait(false);*/
      return "";
    }
}

public class CodeExampleRetrievalPlugins(
    BlobServiceClient blobServiceClient,
    SearchClient searchClient,
    IKernelMemory memory, 
    CodeExampleRetrievalPluginOptions options
) {
    [KernelFunction]
    [Description("Get local examples for a given language.")]
    [return: Description("A list of code block examples for a given language.")]
    public async Task<string> GetLocalExamples(
        [Description("The language to get examples for.")] string language,
        CancellationToken cancellationToken)
    {
        var resultString = string.Empty;
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(options.BlobContainerName);
        var resultSegment = blobContainerClient
            .GetBlobsByHierarchyAsync(prefix: language + "/", delimiter: "/", cancellationToken: cancellationToken)
            .AsPages()
            .ConfigureAwait(false);

        await foreach (var page in resultSegment)
        {
          foreach (var blobItem in page.Values)
          {
            // put the blob contents into a string and return it
            if (!blobItem.IsPrefix && blobItem.Blob.Name.EndsWith(".g4"))
            {
              var result = await blobContainerClient
                .GetBlobClient(blobItem.Blob.Name)
                .DownloadContentAsync(cancellationToken)
                .ConfigureAwait(false);
              resultString += result.Value.Content.ToString();
            }
          }
        }
    
        return resultString;
    }

    [KernelFunction]
    [Description("Get indexed examples for a given input and language.")]
    [return: Description("A list of code block examples for a given input and language.")]
    public async Task<IEnumerable<CodeBlock>> GetIndexedExamples(
        [Description("The full user prompt provided by the user.")] string userPrompt,
        [Description("The language to get examples for.")] string language,
        CancellationToken cancellationToken
    ) {
        List<CodeBlock> examples = new List<CodeBlock>();
        SearchResults<CodeExample> response = await searchClient.SearchAsync<CodeExample>(
            userPrompt,
            new SearchOptions
            {
              Filter = $"tags/any(t: t eq 'language:{language}')",
              SemanticSearch = new()
              {
                SemanticConfigurationName = options.SemanticConfigurationName,
                QueryCaption = new(QueryCaptionType.Extractive),
                QueryAnswer = new(QueryAnswerType.Extractive)
              },
              QueryType = SearchQueryType.Semantic
            }
        );

        await foreach (SearchResult<CodeExample> result in response.GetResultsAsync())
        {
            var payloadObject = JsonSerializer.Deserialize<Payload>(result.Document.payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            examples.Add(new CodeBlock(payloadObject.Response, payloadObject.AdditionalDetails, payloadObject.Prompt));
        }

        return examples;
  }
}
