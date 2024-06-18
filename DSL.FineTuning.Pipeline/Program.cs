using System.Reflection;
using Antlr4.Runtime;
using DSL.FineTuning.Pipeline;
using DSL.FineTuning.Pipeline.Services;
using DSL.FineTuning.Pipeline.Extensions;
using DslCopilot.Core.Agents.CodeValidator;
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
using Microsoft.SemanticKernel.Agents;

CancellationTokenSource cancellationToken = new();
ApplicationSettings applicationSettings = GetApplicationSettings();
IKernelBuilder kernelBuilder = CreateKernelKernelBuilder(
    applicationSettings.OpenAI.ModelName, 
    applicationSettings.OpenAI.Endpoint, 
    applicationSettings.OpenAI.Key
);

IHost host = BuildKernel(args, kernelBuilder, applicationSettings);
OpenAIChatService chatService = new(kernelBuilder);

IMediator mediatr = host.Services.GetRequiredService<IMediator>();
List<string> prompts = GetCodeGenerationPrompts(applicationSettings.GlobalSettings.PromptFileLocation);

foreach (var prompt in prompts)
{
    (ChatMessageContent chatMessageContent, ChatHistoryKernelAgent primaryAgent) = await chatService.AgentChat(
        prompt,
        applicationSettings.GlobalSettings.LanguageName,
        cancellationToken.Token
    ).ConfigureAwait(false);

    var promptGeneratedCode = new PromptGeneratedCode(
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
    IConfigurationRoot config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetExecutingAssembly())
        .Build();

    return config.GetSection("ApplicationSettings").Get<ApplicationSettings>();
}

static IKernelBuilder CreateKernelKernelBuilder(
    string modelName,
    string endpoint,
    string key
) {
    var builder = Kernel.CreateBuilder();

    builder.AddAzureOpenAIChatCompletion(
        modelName,
        endpoint,
        key);

    return builder;
}

static IHost BuildKernel(string[] args, IKernelBuilder kernelBuilder, ApplicationSettings applicationSettings)
{
    HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
    var searchConfig = new AzureAISearchConfig
    {
        APIKey = applicationSettings.AzureAISearch.Key,
        Endpoint = applicationSettings.AzureAISearch.Endpoint,
        Auth = AzureAISearchConfig.AuthTypes.APIKey
    };

    var embeddingAzureOpenAIConfig = new AzureOpenAIConfig
    {
        APIKey = applicationSettings.OpenAI.Key,
        APIType = AzureOpenAIConfig.APITypes.TextCompletion,
        Endpoint = applicationSettings.OpenAI.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = applicationSettings.OpenAI.ModelName
    };

    var completionAzureOpenAIConfig = new AzureOpenAIConfig
    {
        APIKey = applicationSettings.OpenAI.Key,
        APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
        Endpoint = applicationSettings.OpenAI.Endpoint,
        Auth = AzureOpenAIConfig.AuthTypes.APIKey,
        Deployment = applicationSettings.OpenAI.ModelName
    };

    var codeValidationRetrievalPluginOptions = new CodeValidationRetrievalPluginOptions(
        new Dictionary<string, Func<string, (Parser parser, ParserRuleContext rule, ErrorListener listener)>>()
        {
            { 
                applicationSettings.GlobalSettings.LanguageName.ToLower(), 
                input => {
                    var charStream = new AntlrInputStream(input);
                    ClassroomLexer classroomLexer = new (charStream);
                    var tokenStream = new CommonTokenStream(classroomLexer);
                    ClassroomParser classroomParser = new(tokenStream);
                    ErrorListener errorListener = new();

                    classroomParser.RemoveErrorListeners();
                    classroomParser.AddErrorListener(errorListener);

                    return (parser: classroomParser, rule: classroomParser.program(), listener: errorListener);
                }
            }
        }
    );

    builder.Services.AddSingleton(applicationSettings.GlobalSettings);
    builder.Services.GenerateAntlrParser(applicationSettings.GlobalSettings.LanguageName.ToLower(),
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

static List<string> GetCodeGenerationPrompts(string promptFileLocation)
{
    string examples = File.ReadAllText(promptFileLocation);
    var deserializer = new DeserializerBuilder()
          .WithNamingConvention(CamelCaseNamingConvention.Instance)
          .Build();

    return deserializer.Deserialize<List<string>>(examples);
}
