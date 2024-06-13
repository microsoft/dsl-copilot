using DslCopilot.Core.Agents;
using DslCopilot.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace DslCopilot.Core;

public static class ServiceCollectionExtensions
{
    public static void AddDslKernelPlugins(this IKernelBuilder kernelBuilder,
        CodeExamplePluginOptions codeExamplePluginOptions,
        GrammarRetrievalPluginOptions grammarRetrievalPluginOptions)
    {
        kernelBuilder.Services
            .AddSingleton(codeExamplePluginOptions)
            .AddSingleton(grammarRetrievalPluginOptions);
        kernelBuilder.Plugins
            .AddFromType<CodeExamplePlugins>()
            .AddFromType<GrammarRetrievalPlugins>();
    }
    public static void AddDslCopilotCore(this IServiceCollection services)
        => services.AddSingleton(x => new AgentFactory(x.GetRequiredService<Kernel>()));
}