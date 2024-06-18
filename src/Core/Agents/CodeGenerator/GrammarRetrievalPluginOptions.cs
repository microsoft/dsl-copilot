namespace DslCopilot.Core.Plugins;

public record GrammarRetrievalPluginOptions(
    string GrammarPath = "grammar",
    string BlobContainerName = "languages");
