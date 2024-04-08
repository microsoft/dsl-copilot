using DslCopilot.Web.FunctionFilters;
using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;

namespace DslCopilot.Web.KernelHelpers
{
  public static class KernelBuilderExtensions
  {
    public static void AddKernelWithCodeGenFilters(this IServiceCollection services, ChatSessionService chatSessionService, AzureOpenAIOptions? openAiOptions)
    {

      if (openAiOptions == null)
      {
        throw new ArgumentNullException(nameof(openAiOptions));
      }

      if (openAiOptions.CompletionDeploymentName == null)
      {
        throw new ArgumentNullException(nameof(openAiOptions.CompletionDeploymentName));
      }

      if (openAiOptions.Endpoint == null)
      {
        throw new ArgumentNullException(nameof(openAiOptions.Endpoint));
      }

      if (openAiOptions.ApiKey == null)
      {
        throw new ArgumentNullException(nameof(openAiOptions.ApiKey));
      }

      var kernelBuilder = Kernel.CreateBuilder();

      kernelBuilder.AddAzureOpenAIChatCompletion(
          deploymentName: openAiOptions.CompletionDeploymentName,
          endpoint: openAiOptions.Endpoint,
          apiKey: openAiOptions.ApiKey
      );

#pragma warning disable SKEXP0050 // Experimental API
      kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
      kernelBuilder.Plugins.AddFromPromptDirectory("plugins");
#pragma warning restore SKEXP0050 // Experimental API


      var kernel = kernelBuilder.Build();
#pragma warning disable SKEXP0001 // Experimental API
      kernel.FunctionFilters.Add(new CodeRetryFunctionFilter(chatSessionService, kernel));
#pragma warning restore SKEXP0001 // Experimental API

      services.AddTransient(_ => kernel);
    }
  }
}
