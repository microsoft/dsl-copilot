using System.Collections.Concurrent;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LanguageService.Targets;

internal partial class ConfigurableLanguageServer(
    Func<InitializeParams, InitializeResult>? initialize,
    Func<CompletionParams, CompletionList>? completionTextDocument,
    IObserver<NotificationParams<InitializedParams>>? initializedSubject,
    IObserver<NotificationParams<object?>>? exitSubject,
    IObserver<NotificationParams<DidOpenTextDocumentParams>>? didOpenTextDocumentSubject,
    IObserver<NotificationParams<DidCloseTextDocumentParams>>? didCloseTextDocumentSubject
) : ObservableObject, ILanguageService
{
    private readonly ConcurrentDictionary<Uri, TextDocumentItem> _documents = [];
    ConcurrentDictionary<Uri, TextDocumentItem> ILanguageService._textDocumentCache => _documents;

    IObserver<NotificationParams<InitializedParams>>? ILinkedObserver<InitializedParams>.Observer { get; }
        = initializedSubject;
    IObserver<NotificationParams<object?>>? ILinkedObserver<object?>.Observer { get; }
        = exitSubject;
    IObserver<NotificationParams<DidOpenTextDocumentParams>>? ILinkedObserver<DidOpenTextDocumentParams>.Observer { get; }
        = didOpenTextDocumentSubject;
    IObserver<NotificationParams<DidCloseTextDocumentParams>>? ILinkedObserver<DidCloseTextDocumentParams>.Observer { get; }
        = didCloseTextDocumentSubject;

    public InitializeResult Initialize(InitializeParams arg)
        => initialize is null
        ? throw new NotImplementedException()
        : initialize(arg);

    public CompletionList CompletionTextDocument(CompletionParams arg)
        => completionTextDocument is null
        ? throw new NotImplementedException()
        : completionTextDocument(arg);
    public void Dispose() { }
}
