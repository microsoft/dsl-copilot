using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using System.Diagnostics;
using System.Text;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace WebAPI.LSP;

public partial class LanguageServer : ObservableObject
{
    private readonly int maxProblems = -1;
    private TextDocumentItem? textDocument = null;
    private string? referenceToFind;
    private int referencesChunkSize;
    private int referencesDelay;
    private int highlightChunkSize;
    private int highlightsDelay;

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
    } = [];

    public IEnumerable<VSSymbolInformation> Symbols
    {
        get;
        set;
    } = [];

    public IEnumerable<VSProjectContext> Contexts
    {
        get;
        set;
    } = [];

    public bool UsePublishModelDiagnostic { get; set; } = true;
    public string LastCompletionRequest { get; set; } = string.Empty;

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

        List<Location> locations = [];

        List<Location> locationsChunk = [];

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            for (var j = 0; j < line.Length; j++)
            {
                var location = GetLocation(textDocument.Uri, line, i, j, referenceWord);

                if (location != null)
                {
                    locations.Add(location);
                    locationsChunk.Add(location);

                    if (locationsChunk.Count == referencesChunkSize)
                    {
                        Debug.WriteLine($"Reporting references of {referenceWord}");
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

        List<DocumentHighlight> highlights = [];
        List<DocumentHighlight> chunk = [];

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
                    var loc = GetLocation(textDocument.Uri, line, i, j, symbolInfo.Name);
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

    public VSProjectContextList GetProjectContexts() => new()
    {
        ProjectContexts = Contexts.ToArray(),
        DefaultIndex = 0
    };

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

        ApplyWorkspaceEditParams parameter = new()
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
    }

    private static Location? GetLocation(
        Uri uri,
        string line, int lineOffset,
        int characterOffset, string wordToMatch)
    {
        var symbolLocation = line[characterOffset..].IndexOf(wordToMatch);
        return new()
        {
            Uri = uri,
            Range = new()
            {
                Start = new(lineOffset, symbolLocation),
                End = new(lineOffset, symbolLocation + wordToMatch.Length)
            }
        };
    }

    private static Range? GetHighlightRange(string line, int lineOffset, ref int characterOffset, string wordToMatch)
    {
        if ((characterOffset + wordToMatch.Length) <= line.Length)
        {
            var subString = line.Substring(characterOffset, wordToMatch.Length);
            if (subString.Equals(wordToMatch, StringComparison.OrdinalIgnoreCase))
            {
                Range range = new()
                {
                    Start = new(lineOffset, characterOffset),
                    End = new(lineOffset, characterOffset + wordToMatch.Length)
                };

                return range;
            }
        }

        return null;
    }

    private static string GetWordAtPosition(Position position, params string[] lines)
    {
        var line = lines.ElementAtOrDefault(position.Line)
            ?? throw new InvalidOperationException("Could not get line at position");

        StringBuilder result = new();

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
