using Microsoft.VisualStudio.LanguageServer.Protocol;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace WebAPI;

public partial class LanguageServer
{
    public void SendDiagnostics() => SendDiagnostics(diagnostics);

    public void SendDiagnostics(List<DiagnosticsInfo> sentDiagnostics)
    {
        if (textDocument == null || sentDiagnostics == null || !UsePublishModelDiagnostic)
        {
            return;
        }

        var lines = textDocument.Text.Split([Environment.NewLine], StringSplitOptions.None);

        List<Diagnostic> diagnostics = [];
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var j = 0;
            while (j < line.Length)
            {
                Diagnostic? diagnostic = null;
                foreach (var tag in sentDiagnostics)
                {
                    diagnostic = GetDiagnostic(line, i, ref j, tag);

                    if (diagnostic != null)
                    {
                        break;
                    }
                }

                if (diagnostic == null)
                {
                    ++j;
                }
                else
                {
                    diagnostics.Add(diagnostic);
                }
            }
        }

        PublishDiagnosticParams parameter = new()
        {
            Uri = textDocument.Uri,
            Diagnostics = [.. diagnostics]
        };

        if (maxProblems > -1)
        {
            parameter.Diagnostics = parameter.Diagnostics.Take(maxProblems).ToArray();
        }

        _ = SendMethodNotificationAsync(Methods.TextDocumentPublishDiagnostics, parameter);
    }

    public void SendDiagnostics(Uri uri)
    {
        if (this.diagnostics == null || !UsePublishModelDiagnostic)
        {
            return;
        }
        if (textDocument == null) throw new InvalidOperationException("textDocument is null");
        var lines = textDocument.Text.Split([Environment.NewLine], StringSplitOptions.None);

        IReadOnlyList<Diagnostic> diagnostics = GetDocumentDiagnostics(lines);

        PublishDiagnosticParams parameter = new()
        {
            Uri = uri,
            Diagnostics = [.. diagnostics]
        };

        if (maxProblems > -1)
        {
            parameter.Diagnostics = parameter.Diagnostics.Take(maxProblems).ToArray();
        }

        _ = SendMethodNotificationAsync(Methods.TextDocumentPublishDiagnostics, parameter);
    }

    private VSDiagnostic? GetDiagnostic(
        string line,
        int lineOffset,
        ref int characterOffset,
        DiagnosticsInfo diagnosticInfo,
        TextDocumentIdentifier? textDocumentIdentifier = null)
    {
        var wordToMatch = diagnosticInfo.Text;
        var context = diagnosticInfo.Context;
        VSProjectContext? requestedContext = null;
        VSDiagnosticProjectInformation? projectAndContext = null;
        if (textDocumentIdentifier != null
            && textDocumentIdentifier is VSTextDocumentIdentifier vsTextDocumentIdentifier
            && vsTextDocumentIdentifier.ProjectContext != null)
        {
            requestedContext = vsTextDocumentIdentifier.ProjectContext;
            projectAndContext = new()
            {
                ProjectName = vsTextDocumentIdentifier.ProjectContext.Label,
                ProjectIdentifier = vsTextDocumentIdentifier.ProjectContext.Id,
                Context = "Win32"
            };
        }

        if ((characterOffset + wordToMatch?.Length) <= line.Length && wordToMatch != null)
        {
            var subString = line.Substring(characterOffset, wordToMatch.Length);
            if (subString.Equals(wordToMatch, StringComparison.OrdinalIgnoreCase) && context == requestedContext)
            {
                VSDiagnostic diagnostic = new()
                {
                    Message = "This is an " + Enum.GetName(diagnosticInfo.Severity),
                    Severity = diagnosticInfo.Severity,
                    Range = new Range
                    {
                        Start = new(lineOffset, characterOffset),
                        End = new(lineOffset, characterOffset + wordToMatch.Length)
                    },
                    Code = "Test" + Enum.GetName(diagnosticInfo.Severity),
                    CodeDescription = new()
                    {
                        Href = new("https://www.microsoft.com")
                    }
                };

                if (projectAndContext != null)
                {
                    diagnostic.Projects = [projectAndContext];
                }

                diagnostic.Identifier = lineOffset + "," + characterOffset + " " + lineOffset + "," + diagnostic.Range.End.Character;
                characterOffset += wordToMatch.Length;

                // Our Mock UI only allows setting one tag at a time
                diagnostic.Tags = new DiagnosticTag[1];
                diagnostic.Tags[0] = diagnosticInfo.Tag;

                return diagnostic;
            }
        }

        return null;
    }

    public void SendDiagnostics(List<DiagnosticsInfo> sentDiagnostics, bool pushDiagnostics)
    {
        diagnostics ??= sentDiagnostics;

        if (pushDiagnostics)
        {
            SendDiagnostics(sentDiagnostics);
        }
    }

    private IReadOnlyList<VSDiagnostic> GetDocumentDiagnostics(
        string[] lines, TextDocumentIdentifier? textDocumentIdentifier = null)
    {
        List<VSDiagnostic> diagnostics = [];
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            var j = 0;
            while (j < line.Length)
            {
                VSDiagnostic? diagnostic = null;
                foreach (var tag in this.diagnostics)
                {
                    diagnostic = GetDiagnostic(line, i, ref j, tag, textDocumentIdentifier);

                    if (diagnostic != null)
                    {
                        break;
                    }
                }

                if (diagnostic == null)
                {
                    ++j;
                }
                else
                {
                    diagnostics.Add(diagnostic);
                }
            }
        }

        return diagnostics;
    }
}
