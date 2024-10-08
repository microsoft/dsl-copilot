using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.SemanticKernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.TextGeneration;

ConfigurationManager configurationManager = new();
var configuration = configurationManager
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();
var appSettings = configuration.Get<AppSettings>()
    ?? throw new InvalidOperationException("Couldn't bind AppSettings.");
AzureOpenAIClient azureOpenAIClient = new(
    endpoint: new(appSettings.Endpoint),
    credential: new AzureKeyCredential(appSettings.ApiKey)
);

var chatDeployment = appSettings.ChatDeployment;
var embeddingDeployment = appSettings.EmbeddingDeployment;
AzureOpenAIChatCompletionService chatCompletionService = new(chatDeployment, azureOpenAIClient);
AzureOpenAITextEmbeddingGenerationService textEmbeddingGenerationService = new(embeddingDeployment, azureOpenAIClient);

ServiceCollection services = new();
var kernelBuilder = services.AddKernel();
kernelBuilder.Services
    .AddSingleton<IChatCompletionService>(chatCompletionService)
    .AddSingleton<ITextGenerationService>(chatCompletionService)
    .AddSingleton<ITextEmbeddingGenerationService>(textEmbeddingGenerationService)
    .AddKernelMemory(builder =>
    {
        SemanticKernelConfig config = new();
        AzureAISearchConfig azureAISearchConfig = new()
        {
            Endpoint = appSettings.SearchEndpoint,
            APIKey = appSettings.SearchApiKey,
            Auth = AzureAISearchConfig.AuthTypes.APIKey,
            UseHybridSearch = true
        };
        AzureBlobsConfig azureBlobsConfig = new()
        {
            Container = configuration["AzureBlobs:Container"]!,
            Account = configuration["AzureBlobs:Account"]!,
            AccountKey = configuration["AzureBlobs:AccountKey"]!,
            Auth = AzureBlobsConfig.AuthTypes.AccountKey,
            EndpointSuffix = configuration["AzureBlobs:EndpointSuffix"]!
        };
        builder
            .WithCustomTextPartitioningOptions(new()
            {
                // Max 99 tokens per sentence
                MaxTokensPerLine = 99,
                // When sentences are merged into paragraphs (aka partitions), stop at 299 tokens
                MaxTokensPerParagraph = 299,
                // Each paragraph contains the last 47 tokens from the previous one
                OverlappingTokens = 47,
            })
            .WithAzureAISearchMemoryDb(azureAISearchConfig)
            .WithAzureBlobsDocumentStorage(azureBlobsConfig)
            .WithSemanticKernelTextGenerationService(chatCompletionService, config)
            .WithSemanticKernelTextEmbeddingGenerationService(textEmbeddingGenerationService, config);
    });
var kernel = kernelBuilder.Build();

var memory = kernel.GetRequiredService<MemoryServerless>();
//TODO: use memory to ingest documents from an IFileProvider.

file record AppSettings(
    string Endpoint,
    string ApiKey,
    string SearchEndpoint,
    string SearchApiKey,
    string ChatDeployment,
    string EmbeddingDeployment
);
