using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Antlr4.Runtime;

namespace DslCopilot.Web.KernelHelpers;
using Core;
using Core.Plugins;
using Options;
using Services;
using FunctionFilters;
using DslCopilot.Core.Agents.CodeValidator;

public static class KernelBuilderExtensions
{
  public static IServiceCollection GenerateAntlrParser<TLexer, TParser>(
    this IServiceCollection services,
    string language,
    Func<AntlrInputStream?, TLexer> lexerFactory,
    Func<CommonTokenStream?, TParser> parserFactory,
    Func<TParser, ParserRuleContext> ruleFactory)
    where TLexer : Lexer
    where TParser : Parser
    => services.Configure<CodeValidationRetrievalPluginOptions>(o => o.Parsers[language] = (string input) =>
    {
      var charStream = new AntlrInputStream(input);
      var lexer = lexerFactory(charStream);
      var tokenStream = new CommonTokenStream(lexer);
      var parser = parserFactory(tokenStream);
      return (parser, rule: ruleFactory(parser));
    });

  public static IServiceCollection AddKernelWithCodeGenFilters(
    this IServiceCollection services,
    ConsoleService consoleService,
    ChatSessionService chatSessionService,
    AzureOpenAIOptions? openAiOptions,
    LanguageBlobServiceOptions languageBlobServiceOptions,
    CodeValidationRetrievalPluginOptions codeValidationRetrievalPluginOptions)
  {
    ArgumentNullException.ThrowIfNull(openAiOptions);
    Guard.IsNotNull(openAiOptions.EmbeddingDeploymentName, nameof(openAiOptions.EmbeddingDeploymentName));
    Guard.IsNotNull(openAiOptions.CompletionDeploymentName, nameof(openAiOptions.CompletionDeploymentName));
    Guard.IsNotNull(openAiOptions.Endpoint, nameof(openAiOptions.Endpoint));
    Guard.IsNotNull(openAiOptions.ApiKey, nameof(openAiOptions.ApiKey));
    Guard.IsNotNull(openAiOptions.SearchEndpoint, nameof(openAiOptions.SearchEndpoint));
    Guard.IsNotNull(openAiOptions.SearchApiKey, nameof(openAiOptions.SearchApiKey));
    Guard.IsNotNull(languageBlobServiceOptions.AccountName, nameof(languageBlobServiceOptions.AccountName));
    Guard.IsNotNull(languageBlobServiceOptions.AccessKey, nameof(languageBlobServiceOptions.AccessKey));

    services.AddSingleton<PromptBankService>();
    var kernelBuilder = Kernel.CreateBuilder();
    kernelBuilder.AddAzureOpenAIChatCompletion(
        deploymentName: openAiOptions.CompletionDeploymentName,
        endpoint: openAiOptions.Endpoint,
        apiKey: openAiOptions.ApiKey
    );

    var searchConfig = openAiOptions.ToSearchConfig();
    kernelBuilder.Plugins
      .AddFromType<ConversationSummaryPlugin>();
    kernelBuilder.Services
      .AddAzureAISearchAsMemoryDb(searchConfig)
      .AddSingleton<ChatSessionService>()
      .AddSingleton<PromptBankFunctionFilter>()
      .AddKernelMemory(memoryBuilder =>
      {
        var tokenizer = new DefaultGPTTokenizer();
        memoryBuilder
          .WithAzureOpenAITextGeneration(openAiOptions.ToTextGenConfig(), tokenizer)
          .WithAzureOpenAITextEmbeddingGeneration(openAiOptions.ToEmbeddingConfig(), tokenizer)
          .WithAzureAISearchMemoryDb(searchConfig)
          .WithSearchClientConfig(new() { MaxMatchesCount = 3, Temperature = 0.5, TopP = 1 });
      });

    var loggerFactory = LoggerFactory.Create(builder =>
    {
      builder.AddConsole();
      builder.SetMinimumLevel(LogLevel.Debug);
    });

    if (openAiOptions.DebugPrompt == true)
    {
      kernelBuilder.Services
        .AddSingleton(loggerFactory)
        .AddSingleton<DebuggingPromptFilter>();
    }

    kernelBuilder.Services.ConfigureHttpClientDefaults(c =>
    {
      c.AddResilienceHandler("HandleThrottling", static builder =>
      {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
          BackoffType = DelayBackoffType.Exponential,
          ShouldRetryAfterHeader = true,
          MaxRetryAttempts = 5
        });
      });
    });

    kernelBuilder.AddCodeGenAgent(
      new(openAiOptions.SearchEndpoint, openAiOptions.SearchApiKey, "code-index"),
      new(languageBlobServiceOptions.AccountName, languageBlobServiceOptions.AccessKey),
      new(),
      new());
    kernelBuilder.AddCodeValidationAgent(codeValidationRetrievalPluginOptions);

    services.AddTransient(_ => kernelBuilder);
    services.AddTransient(_ =>
    {
      var kernel = kernelBuilder.Build();
      kernel.Plugins.AddFromFunctions("yaml_plugins", [
        kernel.CreateFunctionFromPromptYaml(
          File.ReadAllText("plugins/generateCode.yaml")!,
          promptTemplateFactory: new HandlebarsPromptTemplateFactory()),
      ]);
      T Get<T>() where T : notnull => kernel.Services.GetRequiredService<T>();
      var functionFilters = kernel.FunctionInvocationFilters;
      functionFilters.Add(new CodeRetryFunctionFilter(chatSessionService, consoleService, kernel));
      functionFilters.Add(Get<PromptBankFunctionFilter>());

      if (openAiOptions.DebugPrompt == true)
      {
        var promptFilters = kernel.PromptRenderFilters;
        promptFilters.Add(Get<DebuggingPromptFilter>());
      }
      return kernel;
    });

    T Get<T>(IServiceProvider provider) where T : notnull
      => provider.GetRequiredService<Kernel>().Services.GetRequiredService<T>();
    return services
      .AddTransient(Get<IMemoryDb>)
      .AddTransient(Get<ITextEmbeddingGenerator>)
      .AddTransient(Get<IKernelMemory>)
      .AddSingleton(Get<GrammarRetrievalPlugins>)
      .AddSingleton(Get<CodeExampleRetrievalPlugins>);
  }
}
