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
    public static void AddKernelWithCodeGenFilters(this IServiceCollection services, AzureOpenAIOptions openAiOptions)
    {
      var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );

    //var kernel = kernelBuilder.Build();
    //kernelBuilder.Services.AddTransient(_ => kernel);
    kernelBuilder.Services.AddSingleton<ChatSessionService>();

#pragma warning disable SKEXP0050 // Experimental API
      kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
      kernelBuilder.Plugins.AddFromPromptDirectory("plugins");
#pragma warning restore SKEXP0050 // Experimental API


      var kernel = kernelBuilder.Build();
#pragma warning disable SKEXP0001 // Experimental API
      kernel.FunctionFilters.Add(new CodeRetryFunctionFilter(kernel.GetRequiredService<ChatSessionService>(), kernel));
#pragma warning restore SKEXP0001 // Experimental API

      services.AddTransient(_ => kernel);
    }
  }
}
