using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Embedded;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DslCopilot.Core.Agents;

public class AgentFactory
{
    private static readonly string _namespace = typeof(AgentFactory).Namespace!;
    private const string
        PromptFileName = "prompt.yaml",
        CodeGenPath = "CodeGenerator",
        CodeValidatorPath = "CodeValidator",
        CodeCustodianPath = "CodeCustodian";

    private static string GetFullyQualifiedName(string path, string name)
        => $"{_namespace}.{path.Replace('/', '.').Replace('\\', '.')}.{name}";
    private static KeyValuePair<string, IFileInfo> GetDefaultKeyValuePair(
        Assembly assembly,
        string path,
        string resourceName)
        => new(Path.Combine(path, resourceName),
            assembly.GetManifestResourceInfo(GetFullyQualifiedName(path, resourceName)) switch
            {
                { ResourceLocation: ResourceLocation.Embedded }
                    => new EmbeddedResourceFileInfo(assembly, path, resourceName, DateTimeOffset.UtcNow),
                _ => throw new InvalidOperationException("Resource not found")
            });

    private static Dictionary<string, Dictionary<string, IFileInfo>> GetDefaultFiles(Assembly assembly) => new()
    {
        {CodeGenPath, [GetDefaultKeyValuePair(assembly, CodeGenPath, PromptFileName)]}
    };

    private readonly Dictionary<string, Dictionary<string, IFileInfo>> _files;
    private readonly Kernel _kernel;

    public AgentFactory(Kernel kernel) : this(kernel, Assembly.GetExecutingAssembly()) { }
    internal AgentFactory(Kernel kernel, Assembly assembly) : this(kernel, GetDefaultFiles(assembly)) { }
    internal AgentFactory(Kernel kernel, Dictionary<string, Dictionary<string, IFileInfo>> files)
    {
        _files = files;
        _kernel = kernel;
    }

    private TAgent GetAgentFromFile<TAgent>(string path, string name)
        where TAgent : Agent
    {
        var fileInfo = _files[path][name];
        using var stream = fileInfo.CreateReadStream();
        using StreamReader reader = new(stream);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
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
            ExecutionSettings = agent.ExecutionSettings,
        };
    }

    public Agent CreateCodeGenerator() =>
        GetChatCompletionAgentFromFile(CodeGenPath, PromptFileName, "code-generator");
    public Agent CreateCodeValidator() =>
        GetChatCompletionAgentFromFile(CodeValidatorPath, PromptFileName, "code-validator");
    public Agent CreateCodeCustodian() =>
        GetChatCompletionAgentFromFile(CodeCustodianPath, PromptFileName, "code-custodian");
}
