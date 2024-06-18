using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

namespace DslCopilot.Core.Plugins
{
    public class ParseTreeListener : IParseTreeListener
    {
        public void EnterEveryRule(ParserRuleContext ctx) => Console.WriteLine(ctx.GetText());
        public void ExitEveryRule([NotNull] ParserRuleContext ctx) => Console.WriteLine(ctx.GetText());
        public void VisitErrorNode([NotNull] IErrorNode node) => Console.WriteLine(node.GetText());
        public void VisitTerminal([NotNull] ITerminalNode node) => Console.WriteLine(node.GetText());
    }
}
