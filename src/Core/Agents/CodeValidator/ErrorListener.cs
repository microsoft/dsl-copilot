using Antlr4.Runtime;

namespace DslCopilot.Core.Plugins
{
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
            var message = $"line {line}:{charPositionInLine} {msg}";
            _errors.Add(message);
        }
    }
}
