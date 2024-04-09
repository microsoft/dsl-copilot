using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using DslCopilot.Web.Options;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.Services;
public class LanguageService
{
  private readonly BlobServiceClient _blobServiceClient;
  private readonly BlobContainerClient _blobContainerClient;
  private readonly Dictionary<string, string> _cachedGrammars;

  public LanguageService(IOptions<LanguageBlobServiceOptions> blobServiceOptions)
  {
    Guard.IsNotNull(blobServiceOptions, nameof(blobServiceOptions));
    Guard.IsNotNull(blobServiceOptions.Value, nameof(blobServiceOptions.Value));
    var value = blobServiceOptions.Value;
    Guard.IsNotNull(value.AccountName, nameof(value.AccountName));
    Guard.IsNotNull(value.AccessKey, nameof(value.AccessKey));
    Guard.IsNotNull(value.ContainerName, nameof(value.ContainerName));

    StorageSharedKeyCredential storageSharedKeyCredential =
      new(value.AccountName,
        value.AccessKey);
    string blobServiceEndpoint = $"https://{value.AccountName}.blob.core.windows.net";
    _blobServiceClient = new(new(blobServiceEndpoint), storageSharedKeyCredential);
    _blobContainerClient = _blobServiceClient.GetBlobContainerClient(value.ContainerName);
    _cachedGrammars = [];
  }

  // The supported languages will be the "folders" at the top tier of the blob container
  public async Task<List<string>> GetSupportedLanguages()
  {
    List<string> languages = [];

    try
    {
      var resultSegment = _blobContainerClient.GetBlobsByHierarchyAsync(delimiter: "/").AsPages();
      await foreach (var page in resultSegment)
      {
        foreach (var blobItem in page.Values)
        {
          if (blobItem.IsPrefix)
          {
            var languageName = blobItem.Prefix.Replace("/", "");
            languages.Add(languageName);
          }
        }
      }
    }
    catch (RequestFailedException e)
    {
      //TODO: log this error
      Console.WriteLine(e.Message);
      throw;
    }

    return languages;
  }

  // The grammar file will be an ANTLR (.g4) file in the language folder
  public async Task<string> GetGrammarForLanguage(string language)
  {
    string resultString = string.Empty;

    if (_cachedGrammars.TryGetValue(language, out string? value))
    {
      return value;
    }

    try
    {
      var resultSegment = _blobContainerClient.GetBlobsByHierarchyAsync(prefix: language + "/", delimiter: "/").AsPages();

      await foreach (var page in resultSegment)
      {
        foreach (var blobItem in page.Values)
        {
            // put the blob contents into a string and return it
          if (!blobItem.IsPrefix && blobItem.Blob.Name.EndsWith(".g4"))
          {
            var result = await _blobContainerClient.GetBlobClient(blobItem.Blob.Name).DownloadContentAsync();
            resultString += result.Value.Content.ToString();
          }
        }
      }

      _cachedGrammars.Add(language, resultString);
    }
    catch (RequestFailedException e)
    {
      //TODO: log this error
      Console.WriteLine(e.Message);
      throw;
    }

    return resultString;
  }
}
