using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LSP;

public class DiagnosticsInfo(
    string Text,
    VSProjectContext context,
    DiagnosticTag tag,
    DiagnosticSeverity severity)
{
    public string Text
    {
        get;
        set;
    } = Text;

    public VSProjectContext Context
    {
        get;
        set;
    } = context;

    public DiagnosticTag Tag
    {
        get;
        set;
    } = tag;

    public DiagnosticSeverity Severity
    {
        get;
        set;
    } = severity;
}
