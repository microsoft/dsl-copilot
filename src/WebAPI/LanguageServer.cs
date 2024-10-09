using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;
using System.Diagnostics;
using System.Text;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace WebAPI;

public partial class LanguageServer
{
    private int maxProblems = -1;
    private readonly HeaderDelimitedMessageHandler messageHandler;
    private readonly LanguageServerTarget target;
    private readonly ManualResetEvent disconnectEvent = new(false);
    private List<DiagnosticsInfo> diagnostics;
    private TextDocumentItem? textDocument = null;
    private string? referenceToFind;
    private int referencesChunkSize;
    private int referencesDelay;
    private int highlightChunkSize;
    private int highlightsDelay;
    private int counter = 100;

    public LanguageServer(Stream sender, Stream reader, List<DiagnosticsInfo>? initialDiagnostics = null)
    {
        TraceSource jsonRpcTraceSource = new("JsonRpc", SourceLevels.All);
        target = new(this, jsonRpcTraceSource);
        messageHandler = new(sender, reader);
        ((JsonMessageFormatter)messageHandler.Formatter).JsonSerializer.Converters.Add(
            new VSExtensionConverter<TextDocumentIdentifier, VSTextDocumentIdentifier>());
        rpc = new(messageHandler, target)
        {
            TraceSource = jsonRpcTraceSource,
            ActivityTracingStrategy = new CorrelationManagerTracingStrategy()
            {
                TraceSource = jsonRpcTraceSource,
            }
        };
        rpc.Disconnected += OnRpcDisconnected;
        rpc.StartListening();

        diagnostics = initialDiagnostics ?? [];

        FoldingRanges = [];
        Symbols = [];

        target.OnInitializeCompletion += OnTargetInitializeCompletion;
        target.OnInitialized += OnTargetInitialized;
    }

    public string? CustomText
    {
        get;
        set;
    }

    public string? CurrentSettings
    {
        get; private set;
    }


    public IEnumerable<FoldingRange> FoldingRanges
    {
        get;
        set;
    }

    public IEnumerable<VSSymbolInformation> Symbols
    {
        get;
        set;
    }

    public IEnumerable<VSProjectContext> Contexts
    {
        get;
        set;
    } = [];

    public bool UsePublishModelDiagnostic { get; set; } = true;

    private string lastCompletionRequest = string.Empty;
    public string LastCompletionRequest
    {
        get => lastCompletionRequest;
        set
        {
            lastCompletionRequest = value ?? string.Empty;
            NotifyPropertyChanged(nameof(LastCompletionRequest));
        }
    }


    public void OnTextDocumentOpened(DidOpenTextDocumentParams messageParams)
    {
        textDocument = messageParams.TextDocument;

        SendDiagnostics();
    }

    public void OnTextDocumentClosed(DidCloseTextDocumentParams messageParams) => textDocument = null;

    public void SetFindReferencesParams(string wordToFind, int chunkSize, int delay = 0)
    {
        referenceToFind = wordToFind;
        referencesChunkSize = chunkSize;
        referencesDelay = delay;
    }

    public void SetDocumentHighlightsParams(int chunkSize, int delay = 0)
    {
        highlightChunkSize = chunkSize;
        highlightsDelay = delay * 1000;
    }

    public void UpdateServerSideTextDocument(string text, int version)
    {
        if (textDocument != null)
        {
            textDocument.Text = text;
            textDocument.Version = version;
        }
    }

    public object[] SendReferences(ReferenceParams args, bool returnLocationsOnly, CancellationToken token)
    {
        rpc.TraceSource.TraceEvent(TraceEventType.Information, 0, $"Received: {JToken.FromObject(args)}");

        IProgress<object[]> progress = args.PartialResultToken 
            ?? throw new NullReferenceException();
        var delay = referencesDelay * 1000;

        // Set default values if no custom values are set
        if (referencesChunkSize <= 0 || string.IsNullOrEmpty(referenceToFind))
        {
            referenceToFind = "error";
            referencesChunkSize = 1;
        }

        var referenceWord = referenceToFind;

        if (textDocument == null || progress == null)
        {
            return [];
        }

        var lines = textDocument.Text.Split([Environment.NewLine], StringSplitOptions.None);

        var locations = new List<Location>();

        var locationsChunk = new List<Location>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            for (var j = 0; j < line.Length; j++)
            {
                var location = GetLocation(line, i, ref j, referenceWord);

                if (location != null)
                {
                    locations.Add(location);
                    locationsChunk.Add(location);

                    if (locationsChunk.Count == referencesChunkSize)
                    {
                        Debug.WriteLine($"Reporting references of {referenceWord}");
                        rpc.TraceSource.TraceEvent(TraceEventType.Information, 0, $"Report: {JToken.FromObject(locationsChunk)}");
                        progress.Report([.. locationsChunk]);
                        Thread.Sleep(delay);  // Wait between chunks
                        locationsChunk.Clear();
                    }
                }

                if (token.IsCancellationRequested)
                {
                    Debug.WriteLine($"Cancellation Requested for {referenceWord} references");
                }

                token.ThrowIfCancellationRequested();
            }
        }

        // Report last chunk if it has elements since it didn't reached the specified size
        if (locationsChunk.Count > 0)
        {
            progress.Report([.. locationsChunk]);
            Thread.Sleep(delay);  // Wait between chunks
        }
        
        return [.. locations];
    }

    public void SetFoldingRanges(
        IEnumerable<FoldingRange> foldingRanges) => FoldingRanges = foldingRanges;

    public FoldingRange[] GetFoldingRanges() => FoldingRanges.ToArray();

    public DocumentHighlight[] GetDocumentHighlights(
        IProgress<DocumentHighlight[]> progress, Position position, CancellationToken token)
    {
        if (textDocument == null || progress == null)
        {
            return [];
        }

        var delay = highlightsDelay;
        var lines = textDocument.Text.Split([Environment.NewLine], StringSplitOptions.None);

        // Default to "error" if no word is selected
        var currentHighlightedWord = GetWordAtPosition(position, lines);
        if (string.IsNullOrEmpty(currentHighlightedWord))
        {
            currentHighlightedWord = "error";
        }

        highlightChunkSize = Math.Max(highlightChunkSize, 1);

        var highlights = new List<DocumentHighlight>();
        var chunk = new List<DocumentHighlight>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            for (var j = 0; j < line.Length; j++)
            {
                var range = GetHighlightRange(line, i, ref j, currentHighlightedWord);

                if (range != null)
                {
                    var highlight = new DocumentHighlight() { Range = range, Kind = DocumentHighlightKind.Text };
                    highlights.Add(highlight);
                    chunk.Add(highlight);

                    if (chunk.Count == highlightChunkSize)
                    {
                        progress.Report([.. chunk]);
                        Thread.Sleep(delay);  // Wait between chunks
                        chunk.Clear();
                    }
                }

                token.ThrowIfCancellationRequested();
            }
        }

        // Report last chunk if it has elements since it didn't reached the specified size
        if (chunk.Count > 0)
        {
            progress.Report([.. chunk]);
        }

        return [.. highlights];
    }

    public void SetDocumentSymbols(IEnumerable<VSSymbolInformation> symbolsInfo)
    {
        if (textDocument == null)
        {
            Symbols = [];
            return;
        }

        var lines = textDocument.Text.Split([Environment.NewLine], StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            for (var j = 0; j < line.Length; j++)
            {

                foreach (var symbolInfo in symbolsInfo)
                {
                    Location? loc = null;

                    loc = GetLocation(line, i, ref j, symbolInfo.Name);

                    if (loc != null)
                    {
                        symbolInfo.Location = loc;
                    }
                }

            }
        }

        Symbols = symbolsInfo;
    }

    public VSSymbolInformation[] GetDocumentSymbols() => Symbols.ToArray();

    public void SetProjectContexts(IEnumerable<VSProjectContext> contexts) => Contexts = contexts;

    public VSProjectContextList GetProjectContexts()
    {
        var result = new VSProjectContextList();
        result.ProjectContexts = Contexts.ToArray();
        result.DefaultIndex = 0;

        return result;
    }

    public void ShowMessage(string message, MessageType messageType)
    {
        var parameter = new ShowMessageParams
        {
            Message = message,
            MessageType = messageType
        };
        _ = SendMethodNotificationAsync(Methods.WindowShowMessage, parameter);
    }

    public async Task<MessageActionItem> ShowMessageRequestAsync(string message, MessageType messageType, string[] actionItems)
    {
        var parameter = new ShowMessageRequestParams
        {
            Message = message,
            MessageType = messageType,
            Actions = actionItems.Select(a => new MessageActionItem { Title = a }).ToArray()
        };

        return await SendMethodRequestAsync(Methods.WindowShowMessageRequest, parameter);
    }

    public void SendSettings(DidChangeConfigurationParams parameter)
    {
        CurrentSettings = parameter.Settings.ToString()
            ?? throw new NullReferenceException();
        NotifyPropertyChanged(nameof(CurrentSettings));

        var parsedSettings = JToken.Parse(CurrentSettings);
        var newMaxProblems = parsedSettings.Children().First().Values<int>("maxNumberOfProblems").First();
        if (maxProblems != newMaxProblems)
        {
            maxProblems = newMaxProblems;
            SendDiagnostics();
        }
    }

    public void ApplyTextEdit(string text)
    {
        if (textDocument == null)
        {
            return;

        }
        TextEdit[] addTextEdit =
        [
            new()
            {
                Range = new()
                {
                    Start = new()
                    {
                        Line = 0,
                        Character = 0
                    },
                    End = new()
                    {
                        Line = 0,
                        Character = 0
                    }
                },
                NewText = text,
            }
        ];

        var parameter = new ApplyWorkspaceEditParams()
        {
            Label = "Test Edit",
            Edit = new()
            {
                DocumentChanges = new TextDocumentEdit[]
                    {
                        new()
                        {
                            TextDocument = new OptionalVersionedTextDocumentIdentifier()
                            {
                                Uri = textDocument.Uri,
                            },
                            Edits = addTextEdit,
                        },
                    }
            }
        };

        _ = Task.Run(async () =>
        {
            var response = await SendMethodRequestAsync(Methods.WorkspaceApplyEdit, parameter);

            if (!response.Applied)
            {
                Console.WriteLine($"Failed to apply edit: {response.FailureReason}");
            }
        });
    }

    private Location? GetLocation(string line, int lineOffset, ref int characterOffset, string wordToMatch)
    {
        if ((characterOffset + wordToMatch.Length) <= line.Length)
        {
            var subString = line.Substring(characterOffset, wordToMatch.Length);
            if (subString.Equals(wordToMatch, StringComparison.OrdinalIgnoreCase))
            {
                if(textDocument == null) throw new NullReferenceException();
                return new Location
                {
                    Uri = textDocument.Uri,
                    Range = new()
                    {
                        Start = new(lineOffset, characterOffset),
                        End = new(lineOffset, characterOffset + wordToMatch.Length)
                    }
                };
            }
        }

        return null;
    }

    private Range? GetHighlightRange(string line, int lineOffset, ref int characterOffset, string wordToMatch)
    {
        if ((characterOffset + wordToMatch.Length) <= line.Length)
        {
            var subString = line.Substring(characterOffset, wordToMatch.Length);
            if (subString.Equals(wordToMatch, StringComparison.OrdinalIgnoreCase))
            {
                var range = new Range();
                range.Start = new Position(lineOffset, characterOffset);
                range.End = new Position(lineOffset, characterOffset + wordToMatch.Length);

                return range;
            }
        }

        return null;
    }

    private string GetWordAtPosition(Position position, string[] lines)
    {
        var line = lines.ElementAtOrDefault(position.Line)
            ?? throw new InvalidOperationException("Could not get line at position");

        var result = new StringBuilder();

        var startIdx = position.Character;
        var endIdx = startIdx + 1;

        while (char.IsLetter(line.ElementAtOrDefault(startIdx)))
        {
            result.Insert(0, line[startIdx]);
            startIdx--;
        }

        while (char.IsLetter(line.ElementAtOrDefault(endIdx)))
        {
            result.Append(line[endIdx]);
            endIdx++;
        }

        return result.ToString();
    }
}
