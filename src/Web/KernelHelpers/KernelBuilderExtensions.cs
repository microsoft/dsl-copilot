using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.KernelHelpers;
using Options;
using Services;
using FunctionFilters;

public static class KernelBuilderExtensions
{
  public static void AddKernelWithCodeGenFilters(
    this IServiceCollection services,
    ConsoleService consoleService,
    ChatSessionService chatSessionService,
    AzureOpenAIOptions? openAiOptions)
  {
    ArgumentNullException.ThrowIfNull(openAiOptions);
    Guard.IsNotNull(openAiOptions.EmbeddingDeploymentName, nameof(openAiOptions.EmbeddingDeploymentName));
    Guard.IsNotNull(openAiOptions.CompletionDeploymentName, nameof(openAiOptions.CompletionDeploymentName));
    Guard.IsNotNull(openAiOptions.Endpoint, nameof(openAiOptions.Endpoint));
    Guard.IsNotNull(openAiOptions.ApiKey, nameof(openAiOptions.ApiKey));
    Guard.IsNotNull(openAiOptions.SearchEndpoint, nameof(openAiOptions.SearchEndpoint));
    Guard.IsNotNull(openAiOptions.SearchApiKey, nameof(openAiOptions.SearchApiKey));
    
    var aoaiCompletionConfig = new AzureOpenAIConfig
    {
      APIKey = openAiOptions.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
      Endpoint = openAiOptions.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = openAiOptions.CompletionDeploymentName
    };
    // TODO: Create a value copy clone of aoaiEmbeddingConfig
    var aoaiEmbeddingConfig = new AzureOpenAIConfig
    {
      APIKey = openAiOptions.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
      Endpoint = openAiOptions.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = openAiOptions.EmbeddingDeploymentName
    };
    var aoaiSearchConfig = new AzureAISearchConfig
    {
      APIKey = openAiOptions.SearchApiKey,
      Endpoint = openAiOptions.SearchEndpoint,
      Auth = AzureAISearchConfig.AuthTypes.APIKey
    };
    var aoaiTextGenConfig = new AzureOpenAIConfig
    {
      APIKey = openAiOptions.ApiKey,
      APIType = AzureOpenAIConfig.APITypes.TextCompletion,
      Endpoint = openAiOptions.Endpoint,
      Auth = AzureOpenAIConfig.AuthTypes.APIKey,
      Deployment = openAiOptions.CompletionDeploymentName
    };
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );
    kernelBuilder.Plugins
      .AddFromType<ConversationSummaryPlugin>();
    kernelBuilder.Services
      .AddAzureAISearchAsMemoryDb(aoaiSearchConfig)
      .AddSingleton<ChatSessionService>()
      .AddSingleton<PromptBankFunctionFilter>()
      .AddKernelMemory(memoryBuilder =>
      {
        var tokenizer = new DefaultGPTTokenizer();
        memoryBuilder
          .WithAzureOpenAITextGeneration(aoaiTextGenConfig, tokenizer)
          .WithAzureOpenAITextEmbeddingGeneration(aoaiEmbeddingConfig, tokenizer)
          .WithAzureAISearchMemoryDb(aoaiSearchConfig)
          .WithSearchClientConfig(new() { MaxMatchesCount = 3, Temperature = 0.5, TopP = 1 });
      });
    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromFunctions("yaml_plugins", [
      kernel.CreateFunctionFromPromptYaml(
        File.ReadAllText("plugins/generateCode.yaml")!,
        promptTemplateFactory: new HandlebarsPromptTemplateFactory()),
    ]);
    kernel.FunctionFilters.Add(new CodeRetryFunctionFilter(chatSessionService, consoleService, kernel));
    services
      .AddTransient(_ => kernel)
      .AddTransient(_ => kernel.Services.GetRequiredService<IKernelMemory>());
  }
}
