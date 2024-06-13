using System.ComponentModel;
using DslCopilot.Web.Models;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core.Plugins;

public record CodeExamplePluginOptions(
    string ExamplesPath = "examples",
    string CodeIndex = "code-index",
    int Limit = 3);

public record GrammarRetrievalPluginOptions(
    string GrammarPath = "grammar");

public class GrammarRetrievalPlugins(GrammarRetrievalPluginOptions options)
{
    [KernelFunction]
    [Description("Get the contents of a grammar file for given language.")]
    [return: Description("The contents of the grammar file.")]
    public async Task<string> GetGrammarContents(
        [Description("The language to get the grammar for.")] string language,
        CancellationToken cancellationToken)
    {
        var languagePath = Path.Combine(options.GrammarPath, $"{language}.g4");
        if (!File.Exists(languagePath)) return string.Empty;
        return await File
            .ReadAllTextAsync(languagePath, cancellationToken)
            .ConfigureAwait(false);
    }
}

public class CodeExamplePlugins(
    IKernelMemory memory, 
    CodeExamplePluginOptions options)
{
    [KernelFunction]
    [Description("Get local examples for a given language.")]
    [return: Description("A list of code block examples for a given language.")]
    public async Task<IEnumerable<CodeBlock>> GetLocalExamples(
        [Description("The language to get examples for.")] string language,
        CancellationToken cancellationToken)
    {
        var languagePath = Path.Combine(options.ExamplesPath, $"{language}.yaml");
        if (!File.Exists(languagePath)) return [];
        var examples = await File
            .ReadAllTextAsync(languagePath, cancellationToken)
            .ConfigureAwait(false);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        var codeExamples = deserializer.Deserialize<LanguageExamples>(examples);
        return codeExamples.Prompts;
    }

    [KernelFunction]
    [Description("Get indexed examples for a given input and language.")]
    [return: Description("A list of code block examples for a given input and language.")]
    public async Task<IEnumerable<CodeBlock>> GetIndexedExamples(
        [Description("The input to search for.")] string input,
        [Description("The language to get examples for.")] string language,
        CancellationToken cancellationToken)
    {
        var languageFilter = MemoryFilters.ByTag("language", language);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var memories = await memory
          .SearchAsync(query: input, index: options.CodeIndex, limit: options.Limit,
            filter: languageFilter, cancellationToken: cancellationToken)
          .ConfigureAwait(false);

        return memories.Results
          .SelectMany(memory => memory.Partitions)
          .Select(partition => partition.Text)
          .Select(deserializer.Deserialize<CodeBlock>);
    }
}