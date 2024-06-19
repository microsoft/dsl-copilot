using System.ComponentModel;
using Azure.Storage.Blobs;
using Microsoft.CodeAnalysis;
using Microsoft.SemanticKernel;

namespace DslCopilot.Core.Plugins;

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
