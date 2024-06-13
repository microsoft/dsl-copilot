using System.ComponentModel;
using Amazon.Util.Internal;
using DslCopilot.Core.Models;
using Microsoft.Extensions.FileProviders;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core.Plugins;

public record CodeExampleRetrievalPluginOptions(
    string ExamplesPath = "examples",
    string CodeIndex = "code-index",
    int Limit = 3);

public record GrammarRetrievalPluginOptions(
    string GrammarPath = "grammar");

public static class IFileExtensions
{
    public static async Task<string> ReadAllAsync(this IFileInfo file, CancellationToken cancellationToken)
    {
        await using var stream = file.CreateReadStream();
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}

public class GrammarRetrievalPlugins(
    IFileProvider fileProvider,
    GrammarRetrievalPluginOptions options)
{
    [KernelFunction]
    [Description("Get the contents of a grammar file for given language.")]
    [return: Description("The contents of the grammar file.")]
    public async Task<string> GetGrammarContents(
        [Description("The language to get the grammar for.")] string language,
        CancellationToken cancellationToken)
    {
        var languagePath = Path.Combine(options.GrammarPath, $"{language}.g4");
        var file = fileProvider.GetFileInfo(languagePath);
        return !file.Exists
            ? throw new FileNotFoundException(languagePath)
            : await file.ReadAllAsync(cancellationToken)
                .ConfigureAwait(false);
    }
}

public class CodeExampleRetrievalPlugins(
    IFileProvider fileProvider,
    IKernelMemory memory, 
    CodeExampleRetrievalPluginOptions options)
{
    [KernelFunction]
    [Description("Get local examples for a given language.")]
    [return: Description("A list of code block examples for a given language.")]
    public async Task<IEnumerable<CodeBlock>> GetLocalExamples(
        [Description("The language to get examples for.")] string language,
        CancellationToken cancellationToken)
    {
        var languagePath = Path.Combine(options.ExamplesPath, $"{language}.yaml");
        var file = fileProvider.GetFileInfo(languagePath);
        if (!file.Exists) throw new FileNotFoundException(languagePath);
        var examples = await file
            .ReadAllAsync(cancellationToken)
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