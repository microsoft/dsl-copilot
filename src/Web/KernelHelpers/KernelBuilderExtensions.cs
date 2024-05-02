﻿using Microsoft.KernelMemory;
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
    
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );

    var searchConfig = openAiOptions.ToSearchConfig();
    kernelBuilder.Plugins
      .AddFromType<ConversationSummaryPlugin>();
    kernelBuilder.Services
      .AddAzureAISearchAsMemoryDb(searchConfig)
      .AddSingleton<ChatSessionService>()
      .AddSingleton<PromptBankFunctionFilter>()
      .AddKernelMemory(memoryBuilder =>
      {
        var tokenizer = new DefaultGPTTokenizer();
        memoryBuilder
          .WithAzureOpenAITextGeneration(openAiOptions.ToTextGenConfig(), tokenizer)
          .WithAzureOpenAITextEmbeddingGeneration(openAiOptions.ToEmbeddingConfig(), tokenizer)
          .WithAzureAISearchMemoryDb(searchConfig)
          .WithSearchClientConfig(new() { MaxMatchesCount = 3, Temperature = 0.5, TopP = 1 });
      });
    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromFunctions("yaml_plugins", [
      kernel.CreateFunctionFromPromptYaml(
        File.ReadAllText("plugins/generateCode.yaml")!,
        promptTemplateFactory: new HandlebarsPromptTemplateFactory()),
    ]);
    var functionFilters = kernel.FunctionFilters;
    functionFilters.Add(new CodeRetryFunctionFilter(chatSessionService, consoleService, kernel));
    functionFilters.Add(kernel.Services.GetRequiredService<PromptBankFunctionFilter>());
    services
      .AddTransient(_ => kernel)
      .AddTransient(_ => kernel.Services.GetRequiredService<IKernelMemory>());
  }
}
