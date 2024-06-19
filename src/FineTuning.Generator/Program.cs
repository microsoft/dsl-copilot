using System.Reflection;
using Antlr4.Runtime;
using DSL.FineTuning.Pipeline;
using DSL.FineTuning.Pipeline.Services;
using DSL.FineTuning.Pipeline.Extensions;
using DslCopilot.Core.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Elastic.Transport;
using Microsoft.KernelMemory;
using MediatR;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using DSL.FineTuning.Pipeline.Models;
using Microsoft.Toolkit.Diagnostics;

CancellationTokenSource cancellationToken = new();
var applicationSettings = GetApplicationSettings();
var kernelBuilder = CreateKernelKernelBuilder(
    applicationSettings.OpenAI.ModelName,
    applicationSettings.OpenAI.Endpoint,
    applicationSettings.OpenAI.Key
);

var host = BuildKernel(args, kernelBuilder, applicationSettings);
OpenAIChatService chatService = new(kernelBuilder);

var mediatr = host.Services.GetRequiredService<IMediator>();
var prompts = GetCodeGenerationPrompts(applicationSettings.GlobalSettings.PromptFileLocation);

foreach (var prompt in prompts)
{
    (var chatMessageContent, var primaryAgent) = await chatService.AgentChat(
        prompt,
        applicationSettings.GlobalSettings.LanguageName,
        cancellationToken.Token
    ).ConfigureAwait(false);

    Guard.IsNotNullOrWhiteSpace(primaryAgent.Instructions, nameof(primaryAgent.Instructions));
    Guard.IsNotNullOrWhiteSpace(chatMessageContent.AuthorName, nameof(chatMessageContent.AuthorName));
    Guard.IsNotNullOrWhiteSpace(chatMessageContent.Content, nameof(chatMessageContent.Content));
    PromptGeneratedCode promptGeneratedCode = new(
        primaryAgent.Instructions,
        chatMessageContent.AuthorName,
        chatMessageContent.Role.ToString(),
        prompt,
        chatMessageContent.Content
    );

    await mediatr.Publish(promptGeneratedCode);
}

static ApplicationSettings GetApplicationSettings()
{
    var config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();

    return config.GetSection("ApplicationSettings").Get<ApplicationSettings>()
        ?? throw new InvalidOperationException("ApplicationSettings not found in appsettings.json");
}

static IKernelBuilder CreateKernelKernelBuilder(
    string modelName,
    string endpoint,
    string key)
    => Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(
        modelName,
        endpoint,
        key);

static IHost BuildKernel(string[] args, IKernelBuilder kernelBuilder, ApplicationSettings applicationSettings)
{
    var builder = Host.CreateApplicationBuilder(args);
    AzureAISearchConfig searchConfig = new()
    {
        APIKey = applicationSettings.AzureAISearch.Key,
        Endpoint = applicationSettings.AzureAISearch.Endpoint,
        Auth = AzureAISearchConfig.AuthTypes.APIKey
    };

    AzureOpenAIConfig embeddingAzureOpenAIConfig = new()
    {
        APIKey = applicationSettings.OpenAI.Key,
        APIType = AzureOpenAIConfig.APITypes.TextCompletion,
        Endpoint = applicationSettings.OpenAI.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = applicationSettings.OpenAI.ModelName
    };

    AzureOpenAIConfig completionAzureOpenAIConfig = new()
    {
        APIKey = applicationSettings.OpenAI.Key,
        APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
        Endpoint = applicationSettings.OpenAI.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = applicationSettings.OpenAI.ModelName
    };

    CodeValidationRetrievalPluginOptions codeValidationRetrievalPluginOptions = new(
        new Dictionary<string, Func<string, AntlrConfigOptions>>
        {
            {
                applicationSettings.GlobalSettings.LanguageName.ToLower(),
                input =>
                {
                    AntlrInputStream charStream = new(input);
                    ClassroomLexer classroomLexer = new(charStream);
                    CommonTokenStream tokenStream = new(classroomLexer);
                    ClassroomParser classroomParser = new(tokenStream);
                    ErrorListener errorListener = new();

                    classroomParser.RemoveErrorListeners();
                    classroomParser.AddErrorListener(errorListener);

                    return new(parser: classroomParser, rule: classroomParser.program(), listener: errorListener);
                }
            }
        }
    );

    builder.Services.AddSingleton(applicationSettings.GlobalSettings);
    builder.Services.GenerateAntlrParser(
        applicationSettings.GlobalSettings.LanguageName.ToLower(),
        stream => new ClassroomLexer(stream),
        stream => new ClassroomParser(stream),
        parser => parser.program());

    builder.Services.AddKernelWithCodeGenFilters(
        kernelBuilder,
        embeddingAzureOpenAIConfig,
        completionAzureOpenAIConfig,
        searchConfig,
        codeValidationRetrievalPluginOptions,
        applicationSettings.LanguageStorage.AccountName,
        applicationSettings.LanguageStorage.AccessKey
    );

    builder.Services.AddMediatR(cfg =>
    {
        var currentAssembly = Assembly.GetExecutingAssembly();
        cfg.RegisterServicesFromAssemblies(currentAssembly);
    });

    return builder.Build();
}

static string[] GetCodeGenerationPrompts(string promptFileLocation)
{
    var examples = File.ReadAllText(promptFileLocation);
    var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();

    return deserializer.Deserialize<string[]>(examples);
}
