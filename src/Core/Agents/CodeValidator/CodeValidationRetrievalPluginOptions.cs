namespace DslCopilot.Core.Plugins
{
    public record CodeValidationRetrievalPluginOptions(
        IDictionary<string, Func<string, AntlrConfigOptions>> Parsers);
}
