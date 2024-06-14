using System.ComponentModel;
using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Microsoft.SemanticKernel;

namespace DslCopilot.Core.Agents.CodeValidator
{
    public class ParseTreeListener : IParseTreeListener
    {
        public void EnterEveryRule(ParserRuleContext ctx) => Console.WriteLine(ctx.GetText());
        public void ExitEveryRule([NotNull] ParserRuleContext ctx) => Console.WriteLine(ctx.GetText());
        public void VisitErrorNode([NotNull] IErrorNode node) => Console.WriteLine(node.GetText());
        public void VisitTerminal([NotNull] ITerminalNode node) => Console.WriteLine(node.GetText());
    }

    public class ErrorListener: IAntlrErrorListener<IToken>
    {
        private readonly List<string> _errors = [];
        public IEnumerable<string> Errors => _errors;

        public void SyntaxError(
            [NotNull] IRecognizer recognizer,
            [Nullable] IToken offendingSymbol,
            int line, int charPositionInLine,
            [NotNull] string msg,
            [Nullable] RecognitionException e)
        {
            var message = $"line {line}:{charPositionInLine} {msg} :: {offendingSymbol.Text} :: {e.Message}";
            _errors.Add(message);
        }
    }

    public class CodeValidationRetrievalPlugin(Func<string, Parser> parserProvider)
    {
        [KernelFunction]
        [Description("Gets validation feedback for the code.")]
        [return: Description("The validation feedback.")]
        public string GetCodeValidation(
            [Description("The language of the code.")] string language,
            [Description("The code to validate.")] string code)
        {
            var parser = parserProvider(language);
            ErrorListener errorListener = new();
            parser.AddErrorListener(errorListener);
            var rootRule = parser.RuleContext.children.First(x => x.Parent == null);
            var listener = new ParseTreeListener();
            ParseTreeWalker.Default.Walk(listener, rootRule);
            if(errorListener.Errors.Any())
            {
                var message = "The following errors were found:";
                message += string.Join($"{Environment.NewLine} - ", errorListener.Errors);
                return message;
            }
            return "No errors found. ::success::";
        }
    }
}