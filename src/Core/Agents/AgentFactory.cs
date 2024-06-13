using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core;
using Agents;
using DslCopilot.Core.Models;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class AgentFactory
{
    private const string
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
    private readonly Kernel _kernel;

    public AgentFactory(Kernel kernel) : this(kernel, typeof(AgentFactory).Assembly) { }
    internal AgentFactory(Kernel kernel, Assembly assembly) : this(kernel, new CompositeFileProvider([
        new PhysicalFileProvider(Directory.GetCurrentDirectory()),
        new ManifestEmbeddedFileProvider(assembly),
        new EmbeddedFileProvider(assembly)
    ])) { }
    internal AgentFactory(Kernel kernel, IFileProvider provider) : this(kernel, GetDefaultFiles(provider)) { }
    internal AgentFactory(Kernel kernel, Dictionary<string, Dictionary<string, IFileInfo>> files)
    {
        _files = files;
        _kernel = kernel;
    }

    private TAgent GetAgentFromFile<TAgent>(string path, string name)
    {
        var files = _files[path];
        var fileInfo = files[name];
        if(!fileInfo.Exists)
            throw new FileNotFoundException($"File {name} not found in {path}: {fileInfo.PhysicalPath}");
        using var stream = fileInfo.CreateReadStream();
        using StreamReader reader = new(stream);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<TAgent>(reader);
    }

    private ChatCompletionAgent GetChatCompletionAgentFromFile(string path, string resourceName, string name)
    {
        var agent = GetAgentFromFile<AgentConfig>(path, resourceName);
        return new ChatCompletionAgent
        {
            Name = name,
            Kernel = _kernel,
            Description = agent.Description,
            Instructions = agent.Instructions,
            ExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ModelId = "gpt-4o",
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            }
        };
    }

    public Agent CreateCodeGenerator() =>
        GetChatCompletionAgentFromFile(CodeGenPath, PromptFileName, "code-generator");
    public Agent CreateCodeValidator() =>
        GetChatCompletionAgentFromFile(CodeValidatorPath, PromptFileName, "code-validator");
    public Agent CreateCodeCustodian() =>
        GetChatCompletionAgentFromFile(CodeCustodianPath, PromptFileName, "code-custodian");
}
