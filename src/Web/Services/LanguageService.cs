using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DslCopilot.Web.Options;
using Microsoft.Extensions.Options;

namespace DslCopilot.Web.Services
{
  public class LanguageService
  {
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;

    private Dictionary<string, string> _cachedGrammars;

    public LanguageService(IOptions<LanguageBlobServiceOptions> blobServiceOptions)
    {

      if (blobServiceOptions.Value.AccountName == null)
      {
        throw new ArgumentNullException(nameof(blobServiceOptions.Value.AccountName));
      }

      if (blobServiceOptions.Value.AccessKey == null)
      {
        throw new ArgumentNullException(nameof(blobServiceOptions.Value.AccessKey));
      }

      if (blobServiceOptions.Value.ContainerName == null)
      {
        throw new ArgumentNullException(nameof(blobServiceOptions.Value.ContainerName));
      }

      StorageSharedKeyCredential storageSharedKeyCredential =
        new StorageSharedKeyCredential(
          blobServiceOptions.Value.AccountName,
          blobServiceOptions.Value.AccessKey);

      string blobServiceEndpoint = $"https://{blobServiceOptions.Value.AccountName}.blob.core.windows.net";

      _blobServiceClient = new BlobServiceClient(new Uri(blobServiceEndpoint), storageSharedKeyCredential);

      _blobContainerClient = _blobServiceClient.GetBlobContainerClient(blobServiceOptions.Value.ContainerName);

      _cachedGrammars = new Dictionary<string, string>();
    }

    // The supported languages will be the "folders" at the top tier of the blob container
    public async Task<List<string>> GetSupportedLanguages()
    {
      List<string> languages = new List<string>();

      try
      {
        var resultSegment = _blobContainerClient.GetBlobsByHierarchyAsync(delimiter: "/").AsPages();

        await foreach (Page<BlobHierarchyItem> page in resultSegment)
        {
          foreach (BlobHierarchyItem blobItem in page.Values)
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
        Console.WriteLine(e.Message);
        throw;
      }

      return languages;
    }

    // The grammar file will be an ANTLR (.g4) file in the language folder
    public async Task<string> GetGrammarForLanguage(string language)
    {
      string resultString = string.Empty;

      if (_cachedGrammars.ContainsKey(language))
      {
        return _cachedGrammars[language];
      }

      try
      {
        var resultSegment = _blobContainerClient.GetBlobsByHierarchyAsync(prefix: language + "/", delimiter:"/").AsPages();

        await foreach(Page<BlobHierarchyItem> page in resultSegment)
        {
          foreach (BlobHierarchyItem blobItem in page.Values)
          {
            if (!blobItem.IsPrefix)
            {
              // put the blob contents into a string and return it
              if (blobItem.Blob.Name.EndsWith(".g4"))
              {
                var result = await _blobContainerClient.GetBlobClient(blobItem.Blob.Name).DownloadContentAsync();
                resultString += result.Value.Content.ToString();
              }
            }
          }
        }

        _cachedGrammars.Add(language, resultString);

      } catch (RequestFailedException e)
      {
        Console.WriteLine(e.Message);
        throw;
      }

      return resultString;
    }


  }
}
