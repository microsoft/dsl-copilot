using DslCopilot.Core;
using DslCopilot.Core.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.KernelMemory.AI.OpenAI;
using Polly;
using Microsoft.Extensions.Http.Resilience;
using Antlr4.Runtime;

namespace DSL.FineTuning.Pipeline.Extensions
{
    public static class ServiceCollectionExtensions
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
              AntlrInputStream charStream = new(input);
              var lexer = lexerFactory(charStream);
              CommonTokenStream tokenStream = new(lexer);
              var parser = parserFactory(tokenStream);

              ErrorListener errorListener = new();

              parser.RemoveErrorListeners();
              parser.AddErrorListener(errorListener);

              return new(parser, rule: ruleFactory(parser), errorListener);
          });

        public static IServiceCollection AddKernelWithCodeGenFilters(
            this IServiceCollection services,
            IKernelBuilder kernelBuilder,
            AzureOpenAIConfig embeddingAzureOpenAIConfig,
            AzureOpenAIConfig completionAzureOpenAIConfig,
            AzureAISearchConfig searchConfig,
            CodeValidationRetrievalPluginOptions codeValidationRetrievalPluginOptions,
            string languageBlobAccountName,
            string languageBlobAccessKey
        ) {
            Guard.IsNotNull(embeddingAzureOpenAIConfig, nameof(embeddingAzureOpenAIConfig));
            Guard.IsNotNull(completionAzureOpenAIConfig, nameof(completionAzureOpenAIConfig));
            Guard.IsNotNull(searchConfig, nameof(searchConfig));

            kernelBuilder.AddAzureOpenAIChatCompletion(
                deploymentName: completionAzureOpenAIConfig.Deployment,
                endpoint: completionAzureOpenAIConfig.Endpoint,
                apiKey: completionAzureOpenAIConfig.APIKey
            );

            kernelBuilder.Plugins.AddFromType<ConversationSummaryPlugin>();
            kernelBuilder.Services
              .AddAzureAISearchAsMemoryDb(searchConfig)
              .AddKernelMemory(memoryBuilder =>
              {
                  DefaultGPTTokenizer tokenizer = new();

                  memoryBuilder.WithAzureOpenAITextGeneration(completionAzureOpenAIConfig, tokenizer)
                    .WithAzureOpenAITextEmbeddingGeneration(embeddingAzureOpenAIConfig, tokenizer)
                    .WithAzureAISearchMemoryDb(searchConfig)
                    .WithSearchClientConfig(new() { MaxMatchesCount = 3, Temperature = 0.5, TopP = 1 });
              });

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

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
              new(searchConfig.Endpoint, searchConfig.APIKey, "code-index"),
              new(languageBlobAccountName, languageBlobAccessKey),
              new(),
              new());

            kernelBuilder.AddCodeValidationAgent(codeValidationRetrievalPluginOptions);
            services.AddTransient(_ => kernelBuilder);

            return services
              .AddTransient(GetServiceProvider<IMemoryDb>)
              .AddTransient(GetServiceProvider<ITextEmbeddingGenerator>)
              .AddTransient(GetServiceProvider<IKernelMemory>)
              .AddSingleton(GetServiceProvider<GrammarRetrievalPlugins>)
              .AddSingleton(GetServiceProvider<CodeExampleRetrievalPlugins>);
        }

        static T GetServiceProvider<T>(IServiceProvider provider) where T : notnull
              => provider.GetRequiredService<Kernel>().Services.GetRequiredService<T>();
    }
}
