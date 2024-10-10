using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace WebAPI.LSP;

public class LanguageServerTarget(LanguageServer server, TraceSource traceSource)
{
    private int version = 1;
    private int completionItemsNumberRoot = 0;

    public event EventHandler OnInitializeCompletion = delegate { };

    public event EventHandler OnInitialized = delegate { };

    public bool SuggestionMode
    {
        get;
        set;
    } = false;

    public bool IsIncomplete
    {
        get;
        set;
    } = false;

    public bool CompletionServerError
    {
        get;
        set;
    } = false;

    public bool ServerCommitCharacters { get; internal set; } = true;
    public bool ItemCommitCharacters { get; internal set; } = false;

    private readonly string[] ServerCommitCharacterArray = [" ", "[", "]", "(", ")", ";", "."];
    private readonly string[] ItemCommitCharacterArray = [" ", "[", "]", "(", ")", ";", "-"];

    [JsonRpcMethod(Methods.InitializeName)]
    public InitializeResult Initialize(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");

        MockServerCapabilities capabilities = new()
        {
            TextDocumentSync = new()
            {
                OpenClose = true,
                Change = TextDocumentSyncKind.Full
            },
            CompletionProvider = new()
            {
                ResolveProvider = false,
                TriggerCharacters = [",", ".", "@"],
                AllCommitCharacters = ServerCommitCharacterArray
            },
            SignatureHelpProvider = new MockSignatureHelpOptions()
            {
                TriggerCharacters = ["(", ","],
                RetriggerCharacters = [")"],
                MockSignatureHelp = true,
            },
            RenameProvider = true,
            FoldingRangeProvider = new(),
            ReferencesProvider = true,
            DocumentHighlightProvider = true,
            DocumentSymbolProvider = true,
            CodeActionProvider = new CodeActionOptions() { ResolveProvider = true },
            ProjectContextProvider = true,
            HoverProvider = true,
            Mock = true
        };
        InitializeResult result = new()
        {
            Capabilities = capabilities
        };

        OnInitializeCompletion?.Invoke(this, new EventArgs());

        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");

        return result;
    }

    [JsonRpcMethod(Methods.InitializedName)]
    public void Initialized(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        OnInitialized?.Invoke(this, EventArgs.Empty);
    }

    [JsonRpcMethod(Methods.TextDocumentDidOpenName)]
    public void OnTextDocumentOpened(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<DidOpenTextDocumentParams>()
            ?? throw new InvalidOperationException();
        Debug.WriteLine($"Document Open: {parameter.TextDocument.Uri.AbsolutePath}");
        server.OnTextDocumentOpened(parameter);
    }

    [JsonRpcMethod(Methods.TextDocumentHoverName)]
    public Hover OnHover(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var result = new Hover()
        {
            Contents = new SumType<string, MarkedString>("Mock Hover"),
            Range = new Range()
            {
                Start = new Position(0, 0),
                End = new Position(0, 10),
            },
        };

        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");

        return result;
    }

    [JsonRpcMethod(Methods.TextDocumentDidChangeName)]
    public void OnTextDocumentChanged(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<DidChangeTextDocumentParams>()
            ?? throw new InvalidOperationException("Could not convert to DidChangeTextDocumentParams");
        Debug.WriteLine($"Document Change: {parameter.TextDocument.Uri.AbsolutePath}");
        server.UpdateServerSideTextDocument(parameter.ContentChanges[0].Text, parameter.TextDocument.Version);
        server.SendDiagnostics(parameter.TextDocument.Uri);
    }

    [JsonRpcMethod(Methods.TextDocumentDidCloseName)]
    public void OnTextDocumentClosed(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<DidCloseTextDocumentParams>()
            ?? throw new InvalidOperationException("Could not convert to DidCloseTextDocumentParams");
        Debug.WriteLine($"Document Close: {parameter.TextDocument.Uri.AbsolutePath}");
        server.OnTextDocumentClosed(parameter);
    }

    [JsonRpcMethod(Methods.TextDocumentReferencesName, UseSingleObjectParameterDeserialization = true)]
    public object[] OnTextDocumentFindReferences(ReferenceParams parameter, CancellationToken token)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {JToken.FromObject(parameter)}");
        var result = server.SendReferences(parameter, returnLocationsOnly: true, token: token);
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");
        return result;
    }

    [JsonRpcMethod(Methods.TextDocumentCodeActionName)]
    public object GetCodeActions(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<CodeActionParams>() ?? throw new InvalidCastException();
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(parameter)}");
        return parameter;
    }

    [JsonRpcMethod(Methods.CodeActionResolveName)]
    public object GetResolvedCodeAction(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<CodeAction>() ?? throw new InvalidCastException();
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(parameter)}");
        return parameter;
    }

    [JsonRpcMethod(Methods.TextDocumentCompletionName)]
    public CompletionList OnTextDocumentCompletion(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<CompletionParams>() ?? throw new InvalidCastException();
        var items = new List<CompletionItem>();

        server.LastCompletionRequest = arg.ToString();
        var allKinds = Enum.GetValues<CompletionItemKind>();
        var itemName = IsIncomplete ? "Incomplete" : "Item";
        for (var i = 0; i < 10; i++)
        {
            CompletionItem item = new()
            {
                Label = $"{itemName} {i + completionItemsNumberRoot}",
                InsertText = $"{itemName}{i + completionItemsNumberRoot}"
            };
            item.SortText = item.Label;
            item.Kind = allKinds[(completionItemsNumberRoot + i) % allKinds.Length];
            item.Detail = $"Detail for {itemName} {i + completionItemsNumberRoot}";
            item.Documentation = $"Documentation for {itemName} {i + completionItemsNumberRoot}";
            if (ItemCommitCharacters)
            {
                item.CommitCharacters = ItemCommitCharacterArray;
            }
            else
            {
                item.CommitCharacters = null;
            }

            items.Add(item);
        }

        completionItemsNumberRoot += 10;

        // Items to test sorting, when SortText is equal items should be ordered by label
        // So the following 3 items will be sorted: B, A, C.
        // B comes first being SortText the first sorting criteria
        // Then A and C have same SortText, so they are sorted by Label coming A before C
        CompletionItem cItem = new()
        {
            Label = "C",
            InsertText = "C Kind",
            SortText = "2",
            Kind = 0
        };
        items.Add(cItem);

        CompletionItem bItem = new()
        {
            Label = "B",
            InsertText = "B Kind",
            SortText = "1",
            Kind = 0
        };
        items.Add(bItem);

        CompletionItem aItem = new()
        {
            Label = "A",
            InsertText = "A Kind",
            SortText = "2",
            Kind = 0
        };
        items.Add(aItem);

        CompletionItem invalidItem = new()
        {
            Label = "Invalid",
            InsertText = "Invalid Kind",
            SortText = "Invalid",
            Kind = 0
        };
        items.Add(invalidItem);

        var fileNames = new[] { "sample.txt", "myHeader.h", "src/Feature/MyClass.cs", "../resources/img/sample.png", "http://contoso.com/awesome/Index.razor", "http://schemas.microsoft.com/winfx/2006/xaml/file.xml" };
        for (var i = 0; i < fileNames.Length; i++)
        {
            CompletionItem item = new()
            {
                Label = fileNames[i],
                InsertText = fileNames[i],
                SortText = fileNames[i],
                Kind = CompletionItemKind.File,
                Documentation = $"Verifies whether IVsImageService provided correct icon for {fileNames[i]}",
                CommitCharacters = ["."]
            };
            items.Add(item);
        }

        CompletionList list = new()
        {
            IsIncomplete = IsIncomplete || parameter.Context?.TriggerCharacter == "@" || parameter.Context?.TriggerKind == CompletionTriggerKind.TriggerForIncompleteCompletions && completionItemsNumberRoot % 50 != 0,
            Items = [.. items],
        };

        if (CompletionServerError)
        {
            throw new InvalidOperationException("Simulated server error.");
        }

        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(list)}");

        return list;
    }

    private int callCounter = 0;
    [JsonRpcMethod(Methods.TextDocumentSignatureHelpName)]
    public SignatureHelp? OnTextDocumentSignatureHelp(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");

        SignatureHelp? retVal = null;
        if (callCounter < 4)
        {
            retVal = new()
            {
                ActiveParameter = callCounter % 2,
                ActiveSignature = callCounter / 2,
                Signatures =
                [
                    new()
                    {
                        Label = "foo(param1, param2)",
                        Parameters =
                        [
                            new()
                            {
                                Label = "param1"
                            },
                            new()
                            {
                                Label = "param2"
                            }
                        ],
                    },
                    new()
                    {
                        Label = "foo(param1, param2, param3)",
                        Parameters =
                        [
                            new()
                            {
                                Label = "param1"
                            },
                            new()
                            {
                                Label = "param2"
                            },
                            new()
                            {
                                Label = "param3"
                            }
                        ],
                    }
                ],
            };
        }

        callCounter = (callCounter + 1) % 5;
        if(retVal != null)
            traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(retVal)}");

        return retVal;
    }

    [JsonRpcMethod(Methods.WorkspaceDidChangeConfigurationName)]
    public void OnDidChangeConfiguration(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var parameter = arg.ToObject<DidChangeConfigurationParams>()
            ?? throw new InvalidCastException();
    }

    [JsonRpcMethod(Methods.ShutdownName)]
    public object? Shutdown()
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received Shutdown notification");
        return null;
    }

    [JsonRpcMethod(Methods.ExitName)]
    public void Exit() => traceSource.TraceEvent(TraceEventType.Information, 0, $"Received Exit notification");

    [JsonRpcMethod(Methods.TextDocumentRenameName)]
    public WorkspaceEdit Rename(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var renameParams = arg.ToObject<RenameParams>()
            ?? throw new InvalidCastException();
        var fullText = File.ReadAllText(renameParams.TextDocument.Uri.LocalPath);
        var wordToReplace = GetWordAtPosition(fullText, renameParams.Position);
        var placesToReplace = GetWordRangesInText(fullText, wordToReplace);

        WorkspaceEdit result = new()
        {
            DocumentChanges = new TextDocumentEdit[]
            {
                new() {
                    TextDocument = new()
                    {
                        Uri = renameParams.TextDocument.Uri,
                        Version = ++version
                    },
                    Edits = placesToReplace.Select(range =>
                        new TextEdit
                        {
                            NewText = renameParams.NewName,
                            Range = range
                        }).ToArray()
                }
            }
        };

        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");
        return result;
    }

    [JsonRpcMethod(Methods.TextDocumentFoldingRangeName)]
    public object GetFoldingRanges(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        return server.GetFoldingRanges();
    }

    [JsonRpcMethod(VSMethods.GetProjectContextsName)]
    public object GetProjectContexts(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        var result = server.GetProjectContexts();
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");
        return result;
    }

    [JsonRpcMethod(Methods.TextDocumentDocumentSymbolName)]
    public object GetDocumentSymbols(JToken arg)
    {
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {arg}");
        return server.GetDocumentSymbols();
    }

    [JsonRpcMethod(Methods.TextDocumentDocumentHighlightName, UseSingleObjectParameterDeserialization = true)]
    public DocumentHighlight[] GetDocumentHighlights(DocumentHighlightParams arg, CancellationToken token)
    {
        Contract.Assert(arg.PartialResultToken != null);
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {JToken.FromObject(arg)}");
        var result = server.GetDocumentHighlights(arg.PartialResultToken, arg.Position, token);
        traceSource.TraceEvent(TraceEventType.Information, 0, $"Sent: {JToken.FromObject(result)}");
        return result;
    }

    public Range[] GetWordRangesInText(string fullText, string word)
    {
        var ranges = new List<Range>();
        var textLines = fullText.Split([Environment.NewLine], StringSplitOptions.None);
        for (var i = 0; i < textLines.Length; i++)
        {
            foreach (Match match in Regex.Matches(textLines[i], word))
            {
                ranges.Add(new Range
                {
                    Start = new Position(i, match.Index),
                    End = new Position(i, match.Index + match.Length)
                });
            }
        }

        return [.. ranges];
    }

    public string GetWordAtPosition(string fullText, Position position)
    {
        var textLines = fullText.Split([Environment.NewLine], StringSplitOptions.None);
        var textAtSpecifiedLine = textLines[position.Line];

        var currentWord = string.Empty;
        for (var i = position.Character; i < textAtSpecifiedLine.Length; i++)
        {
            if (textAtSpecifiedLine[i] == ' ')
            {
                break;
            }
            else
            {
                currentWord += textAtSpecifiedLine[i];
            }
        }

        for (var i = position.Character - 1; i > 0; i--)
        {
            if (textAtSpecifiedLine[i] == ' ')
            {
                break;
            }
            else
            {
                currentWord = textAtSpecifiedLine[i] + currentWord;
            }
        }

        return currentWord;
    }

    public string GetText()
        => string.IsNullOrWhiteSpace(server.CustomText)
            ? "custom text from language server target"
            : server.CustomText;
}