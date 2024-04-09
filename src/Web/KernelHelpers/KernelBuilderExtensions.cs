﻿using DslCopilot.Web.FunctionFilters;
using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.KernelHelpers;
public static class KernelBuilderExtensions
{
  public static void AddKernelWithCodeGenFilters(this IServiceCollection services, AzureOpenAIOptions? openAiOptions)
  {
    ArgumentNullException.ThrowIfNull(openAiOptions);
    Guard.IsNotNull(openAiOptions.CompletionDeploymentName, nameof(openAiOptions.CompletionDeploymentName));
    Guard.IsNotNull(openAiOptions.Endpoint, nameof(openAiOptions.Endpoint));
    Guard.IsNotNull(openAiOptions.ApiKey, nameof(openAiOptions.ApiKey));

    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );

    kernelBuilder.Services.AddSingleton<ChatSessionService>();

    kernelBuilder.Plugins
      .AddFromType<ConversationSummaryPlugin>();

    var kernel = kernelBuilder.Build();
    kernel.Plugins.AddFromFunctions("yaml_plugins", [
      kernel.CreateFunctionFromPromptYaml(
        File.ReadAllText("plugins/generateCode.yaml")!,
        promptTemplateFactory: new HandlebarsPromptTemplateFactory()),
    ]);
    kernel.FunctionFilters.Add(new CodeRetryFunctionFilter(kernel.GetRequiredService<ChatSessionService>(), kernel));

    services.AddTransient(_ => kernel);
  }
}
