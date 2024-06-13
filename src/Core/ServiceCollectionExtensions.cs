using Azure.Search.Documents;
using Azure;
using DslCopilot.Core.Agents;
using DslCopilot.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using Azure.Storage.Blobs;
using Azure.Storage;

namespace DslCopilot.Core;

public static class ServiceCollectionExtensions
{
    public static void AddDslKernelPlugins(this IKernelBuilder kernelBuilder,
        string aiSearchEndpoint,
        string aiSearchKey,
        string accountName,
        string accessKey,
        CodeExampleRetrievalPluginOptions codeExamplePluginOptions,
        GrammarRetrievalPluginOptions grammarRetrievalPluginOptions)
    {
        //AI Search
        AzureKeyCredential credential = new AzureKeyCredential(aiSearchKey);
        SearchClient client = new SearchClient(new Uri(aiSearchEndpoint), codeExamplePluginOptions.CodeIndex, credential);

        //Blob Client
        StorageSharedKeyCredential storageSharedKeyCredential = new(accountName, accessKey);
        var blobServiceEndpoint = $"https://{accountName}.blob.core.windows.net";
        BlobServiceClient blobClient = new(new(blobServiceEndpoint), storageSharedKeyCredential);

        var selfAssembly = typeof(ServiceCollectionExtensions).Assembly;
        kernelBuilder.Services
            .AddSingleton(client)
            .AddSingleton(blobClient)
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
