using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI;

public class DiagnosticsInfo
{
    public DiagnosticsInfo(string Text, VSProjectContext context, DiagnosticTag tag, DiagnosticSeverity severity)
    {
        this.Text = Text;
        Context = context;
        Tag = tag;
        Severity = severity;
    }

    public string Text
    {
        get;
        set;
    }

    public VSProjectContext Context
    {
        get;
        set;
    }

    public DiagnosticTag Tag
    {
        get;
        set;
    }

    public DiagnosticSeverity Severity
    {
        get;
        set;
    }
}
