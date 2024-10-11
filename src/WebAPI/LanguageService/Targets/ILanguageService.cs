using System.Collections.Concurrent;
using System.ComponentModel;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;

namespace WebAPI.LanguageService.Targets;

public partial interface ILanguageService
    : INotifyPropertyChanged
{
}

public partial interface ILanguageService
    : ILinkedObserver<InitializedParams>
{
    [JsonRpcMethod(Methods.InitializeName)]
    InitializeResult Initialize(InitializeParams arg); 

    [JsonRpcMethod(Methods.InitializedName)]
    void Initialized(InitializedParams arg)
        => OnNext(Methods.Initialized, arg);
}

public partial interface ILanguageService
    : ILinkedObserver<object?>
{
    [JsonRpcMethod(Methods.ShutdownName)]
    public object? Shutdown()
    {
        Dispose();
        return -1;
    }

    [JsonRpcMethod(Methods.ExitName)]
    public void Exit()
    {
        var result = Shutdown();
        OnNext(Methods.Exit, result);
    }
}

public partial interface ILanguageService
    : ILinkedObserver<DidOpenTextDocumentParams>
{
    protected internal ConcurrentDictionary<Uri, TextDocumentItem> _textDocumentCache { get; }

    [JsonRpcMethod(Methods.TextDocumentDidOpenName)]
    public void DidOpenTextDocument(DidOpenTextDocumentParams arg)
    {
        _textDocumentCache.GetOrAdd(arg.TextDocument.Uri, arg.TextDocument);
        OnNext(Methods.TextDocumentDidOpen, arg);
    }
}

public partial interface ILanguageService
    : ILinkedObserver<DidCloseTextDocumentParams>
{
    [JsonRpcMethod(Methods.TextDocumentDidCloseName)]
    public void DidCloseTextDocument(DidCloseTextDocumentParams arg)
    {
        _textDocumentCache.Remove(arg.TextDocument.Uri, out var _);
        OnNext(Methods.TextDocumentDidClose, arg);
    }

    [JsonRpcMethod(Methods.TextDocumentCompletionName)]
    public CompletionList CompletionTextDocument(CompletionParams arg);
}

public partial interface ILanguageService
    : IDisposable
{
}
