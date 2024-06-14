using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core;
using Agents;
using Models;

public class AgentFactory
{
    internal const string
        PromptFileName = "prompt.yaml",
        CodeGenPath = "Agents/CodeGenerator",
        CodeValidatorPath = "Agents/CodeValidator",
        CodeCustodianPath = "Agents/CodeCustodian";

    //TODO: Note that "Core" is used in place of "DslCopilot" in the namespace,
    // because of a bug in Microsoft.Extensions.FileProviders.Embedded.
    // The package doesn't respect the root namespace specified in MSBuild.
    // Assembly.GetManifestResourceNames() returns the full namespace, including the root namespace.
    private static string GetFullyQualifiedName(string path, string name)
        => $"Core.{path.Replace('/', '.').Replace('\\', '.')}.{name}";
    private static KeyValuePair<string, IFileInfo> GetDefaultKeyValuePair(
        string resourceName,
        IFileInfo file)
        => new(resourceName, file);

    private static Dictionary<string, Dictionary<string, IFileInfo>> GetDefaultFiles(IFileProvider fileProvider) => new()
    {
        {
            CodeGenPath, [
                GetDefaultKeyValuePair(PromptFileName,
                    GetFileInfo(fileProvider, CodeGenPath, PromptFileName)),
            ]
        },
    };

    private static IFileInfo GetFileInfo(IFileProvider fileProvider, string path, string name)
    {
        var physicalName = Path.Combine(path, name);
        var embeddedName = GetFullyQualifiedName(path, name);
        return GetFileInfo(fileProvider, name)
            ?? GetFileInfo(fileProvider, physicalName)
            ?? GetFileInfo(fileProvider, embeddedName)
            ?? throw new FileNotFoundException($"File {name} not found in {path} or {embeddedName}");
    }
    private static IFileInfo? GetFileInfo(IFileProvider fileProvider, string resourceName)
    {
        var file = fileProvider.GetFileInfo(resourceName);
        return file.Exists ? file : null;
    }

    private readonly Dictionary<string, Dictionary<string, IFileInfo>> _files;
    private readonly IKernelBuilder _kernelBuilder;

    public AgentFactory(IKernelBuilder kernelBuilder)
        : this(kernelBuilder, typeof(AgentFactory).Assembly) { }
    internal AgentFactory(IKernelBuilder kernelBuilder, Assembly assembly)
        : this(kernelBuilder, new CompositeFileProvider([
        new PhysicalFileProvider(Directory.GetCurrentDirectory()),
        new ManifestEmbeddedFileProvider(assembly),
        new EmbeddedFileProvider(assembly)
    ])) { }
    internal AgentFactory(IKernelBuilder kernelBuilder, IFileProvider provider)
        : this(kernelBuilder, GetDefaultFiles(provider)) { }
    internal AgentFactory(
        IKernelBuilder kernelBuilder,
        Dictionary<string, Dictionary<string, IFileInfo>> files)
    {
        _files = files;
        _kernelBuilder = kernelBuilder;
    }

    private TAgent GetAgentFromFile<TAgent>(string path, string name)
    {
        var files = _files[path];
        var fileInfo = files[name];
        if (!fileInfo.Exists)
            throw new FileNotFoundException($"File {name} not found in {path}: {fileInfo.PhysicalPath}");
        using var stream = fileInfo.CreateReadStream();
        using StreamReader reader = new(stream);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<TAgent>(reader);
    }

    private ChatCompletionAgent GetChatCompletionAgentFromFile(
        string path,
        string resourceName,
        string name,
        Kernel? kernel,
        PromptExecutionSettings? executionSettings)
    {
        var agent = GetAgentFromFile<AgentConfig>(path, resourceName);
        return new()
        {
            Name = name,
            Kernel = kernel ?? _kernelBuilder.Build(),
            Description = agent.Description,
            Instructions = agent.Instructions,
            ExecutionSettings = executionSettings ?? DefaultPromptExecutionSettings,
        };
    }

    private static readonly PromptExecutionSettings? DefaultPromptExecutionSettings
        = new OpenAIPromptExecutionSettings
        {
            ModelId = "gpt-4o",
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
        };

    public Agent CreateCodeGenerator(
        Kernel? kernel = null,
        PromptExecutionSettings? executionSettings = null) =>
        GetChatCompletionAgentFromFile(
            CodeGenPath,
            PromptFileName,
            "code-generator",
            kernel,
            executionSettings);
    public Agent CreateCodeValidator(
        Kernel? kernel = null,
        PromptExecutionSettings? executionSettings = null) =>
        GetChatCompletionAgentFromFile(
            CodeValidatorPath,
            PromptFileName,
            "code-validator",
            kernel,
            executionSettings);
    public Agent CreateCodeCustodian(
        Kernel? kernel = null,
        PromptExecutionSettings? executionSettings = null) =>
        GetChatCompletionAgentFromFile(
            CodeCustodianPath,
            PromptFileName,
            "code-custodian",
            kernel,
            executionSettings);
}
