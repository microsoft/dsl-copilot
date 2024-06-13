using DslCopilot.Core.Agents;
using DslCopilot.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;

namespace DslCopilot.Core;

public static class ServiceCollectionExtensions
{
    public static void AddDslKernelPlugins(this IKernelBuilder kernelBuilder,
        CodeExampleRetrievalPluginOptions codeExamplePluginOptions,
        GrammarRetrievalPluginOptions grammarRetrievalPluginOptions)
    {
        kernelBuilder.Services
            .AddSingleton<IFileProvider>(new CompositeFileProvider(
            [
                new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                new EmbeddedFileProvider(typeof(ServiceCollectionExtensions).Assembly)
            ]))
            .AddSingleton(codeExamplePluginOptions)
            .AddSingleton(grammarRetrievalPluginOptions);
        kernelBuilder.Plugins
            .AddFromType<CodeExampleRetrievalPlugins>()
            .AddFromType<GrammarRetrievalPlugins>();
    }
    public static void AddDslCopilotCore(this IServiceCollection services)
        => services.AddSingleton(x => new AgentFactory(x.GetRequiredService<Kernel>()));
}