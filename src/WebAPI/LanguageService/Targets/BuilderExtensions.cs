using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace WebAPI.LanguageService.Targets;

public static class BuilderExtensions
{
    public static ILanguageServiceBuilder WithInitialize(
        this ILanguageServiceBuilder builder, Func<JToken, InitializeResult>? initialize)
        => builder.With(config => config with { Initialize = initialize });
    
    public static ILanguageServiceBuilder WithInitialized(
        this ILanguageServiceBuilder builder, Action<JToken>? initialized)
        => builder.With(config => config with { Initialized = initialized });

    public static ILanguageServiceBuilder WithTextDocumentOpened(
        this ILanguageServiceBuilder builder, Action<JToken>? textDocumentOpened)
        => builder.With(config => config with { TextDocumentOpened = textDocumentOpened });

    public static ILanguageServiceBuilder WithTextDocumentHover(
        this ILanguageServiceBuilder builder, Func<JToken, Hover>? textDocumentHover)
        => builder.With(config => config with { TextDocumentHover = textDocumentHover });

    public static ILanguageServiceBuilder WithTextDocumentChanged(
        this ILanguageServiceBuilder builder, Action<JToken>? textDocumentChanged)
        => builder.With(config => config with { TextDocumentChanged = textDocumentChanged });

    public static ILanguageServiceBuilder WithTextDocumentClosed(
        this ILanguageServiceBuilder builder, Action<JToken>? textDocumentClosed)
        => builder.With(config => config with { TextDocumentClosed = textDocumentClosed });

    public static ILanguageServiceBuilder WithGetCodeActions(
        this ILanguageServiceBuilder builder, Func<JToken, object>? getCodeActions)
        => builder.With(config => config with { GetCodeActions = getCodeActions });

    public static ILanguageServiceBuilder WithTextDocumentFindReferences(
        this ILanguageServiceBuilder builder,
        Func<ReferenceParams, CancellationToken, object[]>? onTextDocumentFindReferences)
        => builder.With(config => config with { OnTextDocumentFindReferences = onTextDocumentFindReferences });

    public static ILanguageServiceBuilder WithTextDocumentCompletion(
        this ILanguageServiceBuilder builder,
        Func<JToken, CompletionList>? onTextDocumentCompletion)
        => builder.With(config => config with { OnTextDocumentCompletion = onTextDocumentCompletion });

    public static ILanguageServiceBuilder WithTextDocumentSignatureHelp(
        this ILanguageServiceBuilder builder,
        Func<JToken, SignatureHelp?>? onTextDocumentSignatureHelp)
        => builder.With(config => config with { OnTextDocumentSignatureHelp = onTextDocumentSignatureHelp });

    public static ILanguageServiceBuilder WithGetResolvedCodeAction(
        this ILanguageServiceBuilder builder,
        Func<JToken, object>? getResolvedCodeAction)
        => builder.With(config => config with { GetResolvedCodeAction = getResolvedCodeAction });

    public static ILanguageServiceBuilder WithExit(
        this ILanguageServiceBuilder builder, Action? exit)
        => builder.With(config => config with { Exit = exit });

    public static ILanguageServiceBuilder WithOnDidChangeConfiguration(
        this ILanguageServiceBuilder builder,
        Action<JToken>? onDidChangeConfiguration)
        => builder.With(config => config with { OnDidChangeConfiguration = onDidChangeConfiguration });

    public static ILanguageServiceBuilder WithShutdown(
        this ILanguageServiceBuilder builder, Func<object?>? shutdown)
        => builder.With(config => config with { Shutdown = shutdown });

    public static ILanguageServiceBuilder WithGetProjectContexts(
        this ILanguageServiceBuilder builder,
        Func<JToken, object>? getProjectContexts)
        => builder.With(config => config with { GetProjectContexts = getProjectContexts });

    public static ILanguageServiceBuilder WithGetDocumentSymbols(
        this ILanguageServiceBuilder builder,
        Func<JToken, object>? getDocumentSymbols)
        => builder.With(config => config with { GetDocumentSymbols = getDocumentSymbols });
    
    public static ILanguageServiceBuilder WithGetDocumentHighlights(
        this ILanguageServiceBuilder builder,
        Func<DocumentHighlightParams, CancellationToken, DocumentHighlight[]>? getDocumentHighlights)
        => builder.With(config => config with { GetDocumentHighlights = getDocumentHighlights });

    public static ILanguageServiceBuilder WithGetFoldingRanges(
        this ILanguageServiceBuilder builder,
        Func<JToken, object>? getFoldingRanges)
        => builder.With(config => config with { GetFoldingRanges = getFoldingRanges });

    public static ILanguageServiceBuilder WithRename(
        this ILanguageServiceBuilder builder,
        Func<JToken, WorkspaceEdit>? rename)
        => builder.With(config => config with { Rename = rename });

    public static ILanguageServiceBuilder WithOnTextDocumentFindReferences(
        this ILanguageServiceBuilder builder,
        Func<ReferenceParams, CancellationToken, object[]>? onTextDocumentFindReferences)
        => builder.With(config => config with { OnTextDocumentFindReferences = onTextDocumentFindReferences });
}
