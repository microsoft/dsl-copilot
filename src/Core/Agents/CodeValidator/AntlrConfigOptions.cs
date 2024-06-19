using Antlr4.Runtime;

namespace DslCopilot.Core.Plugins
{
    public record AntlrConfigOptions(
        Parser parser,
        ParserRuleContext rule,
        ErrorListener listener);
}
