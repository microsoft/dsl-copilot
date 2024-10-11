using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Subjects;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft;
using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LanguageService.Targets;

internal class MockLanguageServer : ObservableObject, ILanguageService
{
    private readonly string[] ServerCommitCharacterArray = [" ", "[", "]", "(", ")", ";", "."];
    private readonly string[] ItemCommitCharacterArray = [" ", "[", "]", "(", ")", ";", "-"];
    private readonly ConcurrentDictionary<Uri, TextDocumentItem> _documents = [];
    private readonly IObserver<NotificationParams<object?>> defaultObserver;

    public MockLanguageServer(IObserver<NotificationParams<object?>> defaultObserver)
    {
        this.defaultObserver = defaultObserver;
        _initializedObservable = MapObserver<InitializedParams>();
        _exitObserver = MapObserver<object?>();
        _didOpenTextDocumentObserver = MapObserver<DidOpenTextDocumentParams>();
        _didCloseTextDocumentObserver = MapObserver<DidCloseTextDocumentParams>();
    }

    private readonly IObserver<NotificationParams<InitializedParams>> _initializedObservable;
    IObserver<NotificationParams<InitializedParams>>? ILinkedObserver<InitializedParams>.Observer
        => _initializedObservable;

    private readonly IObserver<NotificationParams<object?>> _exitObserver;
    IObserver<NotificationParams<object?>>? ILinkedObserver<object?>.Observer => _exitObserver;

    private readonly IObserver<NotificationParams<DidOpenTextDocumentParams>> _didOpenTextDocumentObserver;
    IObserver<NotificationParams<DidOpenTextDocumentParams>>? ILinkedObserver<DidOpenTextDocumentParams>.Observer
        => _didOpenTextDocumentObserver;

    private readonly IObserver<NotificationParams<DidCloseTextDocumentParams>> _didCloseTextDocumentObserver;
    IObserver<NotificationParams<DidCloseTextDocumentParams>>? ILinkedObserver<DidCloseTextDocumentParams>.Observer
        => _didCloseTextDocumentObserver;

    ConcurrentDictionary<Uri, TextDocumentItem> ILanguageService._textDocumentCache => _documents;

    private IObserver<NotificationParams<T>> MapObserver<T>()
        => Observer.Create<NotificationParams<T>>(next =>
        {
            var (notify, value) = next;
            defaultObserver.OnNext(new(new(notify.Name), value));
        });
    
    public InitializeResult Initialize(InitializeParams _) => new()
    {
        Capabilities = new()
        {
            TextDocumentSync = new()
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Full,
            },
            CompletionProvider = new()
            {
                ResolveProvider = false,
                TriggerCharacters = [",", ".", "@"],
                AllCommitCharacters = ServerCommitCharacterArray,
            },
        }
    };

    public CompletionList CompletionTextDocument(CompletionParams arg)
    {
        var completionItemsNumberRoot = 0;
        var ItemCommitCharacters = true;
        var IsIncomplete = false;
        List<CompletionItem> items = [];
        var allKinds = Enum.GetValues<CompletionItemKind>();
        var itemName = IsIncomplete ? "Incomplete" : "Item";
        for (var i = 0; i < 10; i++, completionItemsNumberRoot++)
        {
            var label = $"{itemName} {i + completionItemsNumberRoot}";
            items.Add(new()
            {
                Label = label,
                InsertText = $"{itemName}{i + completionItemsNumberRoot}",
                Kind = allKinds[(completionItemsNumberRoot + i) % allKinds.Length],
                Detail = $"Detail for {itemName} {i + completionItemsNumberRoot}",
                Documentation = $"Documentation for {itemName} {i + completionItemsNumberRoot}",
                CommitCharacters = ItemCommitCharacters ? ItemCommitCharacterArray : null,
                SortText = label
            });
        }

        static CompletionItem New(string label, string sortText)
            => new()
            {
                Label = label,
                InsertText = $"{label} Kind",
                SortText = sortText,
                Kind = 0
            };
        // Items to test sorting, when SortText is equal items should be ordered by label
        // So the following 3 items will be sorted: B, A, C.
        // B comes first being SortText the first sorting criteria
        // Then A and C have same SortText, so they are sorted by Label coming A before C
        items.AddRange([
            New("C", "2"),
            New("B", "1"),
            New("A", "2"),
            New("Invalid", "Invalid"),
        ]);
        string[] fileNames = [
            "sample.txt",
            "myHeader.h",
            "src/Feature/MyClass.cs",
            "../resources/img/sample.png",
            "http://contoso.com/awesome/Index.razor",
            "http://schemas.microsoft.com/winfx/2006/xaml/file.xml",
        ];
        //var output = filesNames |> x => x * 2 |? x => x % 2 == 0 |+ (sum, next) => sum + next;
        items.AddRange(fileNames.Select(fileName => new CompletionItem
        {
            Label = fileName,
            InsertText = fileName,
            SortText = fileName,
            Kind = CompletionItemKind.File,
            Documentation = $"Verifies whether IVsImageService provided correct icon for {fileName}",
            CommitCharacters = ["."]
        }));

        return new CompletionList
        {
            IsIncomplete = IsIncomplete
                || arg.Context?.TriggerCharacter == "@"
                || arg.Context?.TriggerKind == CompletionTriggerKind.TriggerForIncompleteCompletions,
            Items = [..items],
        };
    }

    public void Dispose() { }
}
