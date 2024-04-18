using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.KernelHelpers;
using FunctionFilters;
using Options;
using Services;

public static class KernelBuilderExtensions
{
  public static void AddKernelWithCodeGenFilters(this IServiceCollection services,
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

    var memoryBuilder = new KernelMemoryBuilder();
    memoryBuilder.WithAzureOpenAITextGeneration(new AzureOpenAIConfig
    {
        APIKey = openAiOptions.ApiKey,
        APIType = AzureOpenAIConfig.APITypes.ChatCompletion,
        Endpoint = openAiOptions.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = openAiOptions.CompletionDeploymentName
    });
    memoryBuilder.WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
    {
        APIKey = openAiOptions.ApiKey,
        APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
        Endpoint = openAiOptions.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = openAiOptions.EmbeddingDeploymentName
    });
    memoryBuilder.WithAzureAISearchMemoryDb(new AzureAISearchConfig
    {
        APIKey = openAiOptions.SearchApiKey,
        Endpoint = openAiOptions.SearchEndpoint,
        Auth = AzureAISearchConfig.AuthTypes.APIKey
    });    
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );

    var memory = memoryBuilder.Build();
    services.AddTransient(provider => memory);
    kernelBuilder.Services.AddTransient(provider => memory);
    kernelBuilder.Services.AddSingleton<ChatSessionService>();
    kernelBuilder.Plugins
      .AddFromType<ConversationSummaryPlugin>();

    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromFunctions("yaml_plugins", [
      kernel.CreateFunctionFromPromptYaml(
        File.ReadAllText("plugins/generateCode.yaml")!,
        promptTemplateFactory: new HandlebarsPromptTemplateFactory()),
    ]);
    kernel.FunctionFilters.Add(new CodeRetryFunctionFilter(chatSessionService, consoleService, kernel));
    kernel.FunctionFilters.Add(new PromptBankFunctionFilter(memory));
    services.AddTransient(_ => kernel);
  }
}
