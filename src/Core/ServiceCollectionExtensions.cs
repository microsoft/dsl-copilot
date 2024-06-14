using Azure;
using Azure.Search.Documents;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace DslCopilot.Core;
using Plugins;

public record SearchClientOptions(string Endpoint, string Key, string Index)
{
    public Uri EndpointUri => new(Endpoint);
    public AzureKeyCredential Credential => new(Key);
}
public record BlobClientOptions(string AccountName, string AccessKey)
{
    public Uri Endpoint => new($"https://{AccountName}.blob.core.windows.net");
    public StorageSharedKeyCredential Credential => new(AccountName, AccessKey);
}
public static class ServiceCollectionExtensions
{
    public static void AddDslKernelPlugins(this IKernelBuilder kernelBuilder,
        SearchClientOptions searchClientOptions,
        BlobClientOptions blobClientOptions,
        CodeExampleRetrievalPluginOptions codeExamplePluginOptions,
        GrammarRetrievalPluginOptions grammarRetrievalPluginOptions)
    {
        var selfAssembly = typeof(ServiceCollectionExtensions).Assembly;
        kernelBuilder.Services
            .AddSingleton(x => new SearchClient(
                searchClientOptions.EndpointUri,
                searchClientOptions.Index,
                searchClientOptions.Credential))
            .AddSingleton(x => new BlobServiceClient(
                blobClientOptions.Endpoint,
                blobClientOptions.Credential))
            .AddSingleton<IFileProvider>(new CompositeFileProvider(
            [
                new PhysicalFileProvider(Directory.GetCurrentDirectory()),
                new ManifestEmbeddedFileProvider(selfAssembly),          
                new EmbeddedFileProvider(selfAssembly)
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
