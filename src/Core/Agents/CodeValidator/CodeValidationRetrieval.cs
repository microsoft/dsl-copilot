using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
            TextWriter output,
            IRecognizer recognizer,
            IToken offendingSymbol,
            int line, int charPositionInLine, string msg,
            RecognitionException e)
            
        {
            var message = $"line {line}:{charPositionInLine} {msg} :: {offendingSymbol.Text} :: {e.Message}";
            _errors.Add(message);
        }
    }

    public record CodeValidationRetrievalPluginOptions(
        IDictionary<string, Func<string, (Parser parser, ParserRuleContext rule)>> Parsers);

    public class CodeValidationRetrievalPlugin(CodeValidationRetrievalPluginOptions options)
    {
        public static Dictionary<string, Func<string, (Parser parser, ParserRuleContext rule)>> DefaultParsers = [];
        [KernelFunction]
        [Description("Gets validation feedback for the code.")]
        [return: Description("The validation feedback.")]
        public string GetCodeValidation(
            [Description("The language of the code.")] string language,
            [Description("The code to validate.")] string code)
        {
            var (parser, rule) = options.Parsers[language](code);
            ErrorListener errorListener = new();
            parser.AddErrorListener(errorListener);
            var listener = new ParseTreeListener();
            ParseTreeWalker.Default.Walk(listener, rule);
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