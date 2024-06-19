namespace DslCopilot.Core.Plugins;

public record CodeExampleRetrievalPluginOptions(
    string ExamplesPath = "examples",
    string CodeIndex = "code-index",
    string SemanticConfigurationName = "code-index-config",
    int Limit = 3);
