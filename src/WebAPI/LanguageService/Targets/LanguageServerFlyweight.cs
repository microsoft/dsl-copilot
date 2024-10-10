using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace WebAPI.LanguageService.Targets;

public record struct LanguageServerFlyweight(
    Func<JToken, InitializeResult>? Initialize,
    Action<JToken>? Initialized,
    Action<JToken>? TextDocumentOpened,
    Func<JToken, Hover>? TextDocumentHover,
    Action? Exit,
    Action<JToken>? TextDocumentChanged,
    Action<JToken>? TextDocumentClosed,
    Func<DocumentHighlightParams, CancellationToken, DocumentHighlight[]>? GetDocumentHighlights,
    Func<JToken, CompletionList>? OnTextDocumentCompletion,
    Func<JToken, SignatureHelp?>? OnTextDocumentSignatureHelp,
    Func<JToken, WorkspaceEdit>? Rename,
    Func<JToken, object>? GetDocumentSymbols,
    Func<JToken, object>? GetFoldingRanges,
    Func<JToken, object>? GetProjectContexts,
    Func<JToken, object>? GetResolvedCodeAction,
    Action<JToken>? OnDidChangeConfiguration,
    Func<object?>? Shutdown,
    Func<ReferenceParams, CancellationToken, object[]>? OnTextDocumentFindReferences,
    Func<JToken, object>? GetCodeActions)
{
    public readonly ServerCapabilities ToServerCapabilities() => new()
    {
        CodeActionProvider = new CodeActionOptions(), //TODO: enumerate code actions.
        CompletionProvider = new CompletionOptions(), //TODO: enumerate completions.
    };
}
