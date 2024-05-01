using Microsoft.KernelMemory;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.KernelHelpers;
using Options;

public static class AzureOpenAIOptionsExtensions
{
  public static AzureOpenAIConfig ToCompletionConfig(this AzureOpenAIOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    Guard.IsNotNull(options.CompletionDeploymentName, nameof(options.CompletionDeploymentName));
    Guard.IsNotNull(options.Endpoint, nameof(options.Endpoint));
    Guard.IsNotNull(options.ApiKey, nameof(options.ApiKey));
    return new AzureOpenAIConfig
    {
      APIKey = options.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
      Endpoint = options.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = options.CompletionDeploymentName
    };
  }
  
  public static AzureOpenAIConfig ToEmbeddingConfig(this AzureOpenAIOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    Guard.IsNotNull(options.EmbeddingDeploymentName, nameof(options.EmbeddingDeploymentName));
    Guard.IsNotNull(options.Endpoint, nameof(options.Endpoint));
    Guard.IsNotNull(options.ApiKey, nameof(options.ApiKey));
    return new AzureOpenAIConfig
    {
      APIKey = options.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
      Endpoint = options.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = options.EmbeddingDeploymentName
    };
  }

  public static AzureOpenAIConfig ToTextGenConfig(this AzureOpenAIOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    Guard.IsNotNull(options.CompletionDeploymentName, nameof(options.CompletionDeploymentName));
    Guard.IsNotNull(options.Endpoint, nameof(options.Endpoint));
    Guard.IsNotNull(options.ApiKey, nameof(options.ApiKey));
    return new AzureOpenAIConfig
    {
      APIKey = options.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.TextCompletion,
      Endpoint = options.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = options.CompletionDeploymentName
    };
  }

  public static AzureAISearchConfig ToSearchConfig(this AzureOpenAIOptions options)
  {
    ArgumentNullException.ThrowIfNull(options);
    Guard.IsNotNull(options.SearchEndpoint, nameof(options.SearchEndpoint));
    Guard.IsNotNull(options.SearchApiKey, nameof(options.SearchApiKey));
    return new AzureAISearchConfig
    {
      APIKey = options.SearchApiKey,
      Endpoint = options.SearchEndpoint,
      Auth = AzureAISearchConfig.AuthTypes.APIKey
    };
  }
}
