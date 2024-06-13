using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core;
using Agents;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class AgentFactory
{
    private static readonly string _namespace = typeof(AgentFactory).Namespace!;
    private const string
        PromptFileName = "prompt.yaml",
        CodeGenPath = "Agents/CodeGenerator",
        CodeValidatorPath = "Agents/CodeValidator",
        CodeCustodianPath = "Agents/CodeCustodian";

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
        var contents = fileProvider.GetDirectoryContents(".");
        var assembly = typeof(AgentFactory).Assembly;
        var otherContents = assembly.GetManifestResourceNames();
        var f = assembly.GetManifestResourceStream(embeddedName);
        if(f is not null)
        {
        using var reader = new StreamReader(f);
        var file = reader.ReadToEnd();
        Console.WriteLine($"File: {file}");
        }

        Console.WriteLine($"Contents: {string.Join(", ", contents.Select(x => x.Name))}");
        Console.WriteLine($"Other Contents: {string.Join(", ", otherContents)}");
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
        //new PhysicalFileProvider(Directory.GetCurrentDirectory()),
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
        where TAgent : Agent
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
        var agent = GetAgentFromFile<ChatCompletionAgent>(path, resourceName);
        return new ChatCompletionAgent
        {
            Name = name,
            Kernel = _kernel,
            Description = agent.Description,
            Instructions = agent.Instructions,
            ExecutionSettings = new OpenAIPromptExecutionSettings() { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
        };
    }

    public Agent CreateCodeGenerator() =>
        GetChatCompletionAgentFromFile(CodeGenPath, PromptFileName, "code-generator");
    public Agent CreateCodeValidator() =>
        GetChatCompletionAgentFromFile(CodeValidatorPath, PromptFileName, "code-validator");
    public Agent CreateCodeCustodian() =>
        GetChatCompletionAgentFromFile(CodeCustodianPath, PromptFileName, "code-custodian");
}
