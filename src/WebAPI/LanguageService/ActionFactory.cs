using Microsoft.VisualStudio.LanguageServer.Protocol;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace WebAPI.LSP;

public static class ActionFactory
{
    public static CodeAction RenameFileAction(Uri createFileUri, Uri renameNewFileUri) =>
            new()
            {
                Title = "Rename <TheCurrentFile>.txt to <TheCurrentFile>_Renamed.txt",
                Edit = new()
                {
                    DocumentChanges = new SumType<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>[]
                    {
                    new RenameFile()
                    {
                        OldUri = createFileUri,
                        NewUri = renameNewFileUri,
                        Options = new()
                        {
                            Overwrite = true,
                        }
                    },
                    }
                },
            };
    public static CodeAction AddTextAction(CodeActionParams parameter, TextEdit[] addTextEdit) =>
            new()
            {
                Title = "Add Text Action - DocumentChanges property",
                Edit = new WorkspaceEdit
                {
                    DocumentChanges = new TextDocumentEdit[]
                        {
                        new()
                        {
                            TextDocument = new OptionalVersionedTextDocumentIdentifier()
                            {
                                Uri = parameter.TextDocument.Uri,
                            },
                            Edits = addTextEdit,
                        },
                        }
                },
                Kind = CodeActionKind.QuickFix,
            };
    public static CodeAction GetAddTextActionChangesProperty(Dictionary<string, TextEdit[]> changes) => new CodeAction
    {
        Title = "Add Text Action - Changes property",
        Edit = new WorkspaceEdit
        {
            Changes = changes,
        },
        Kind = CodeActionKind.QuickFix,
    };
    public static CodeAction GetAddUnderscoreAction(CodeActionParams parameter) => new()
    {
        Title = "Add _",
        Edit = new WorkspaceEdit
        {
            DocumentChanges = new TextDocumentEdit[]
                        {
                        new()
                        {
                            TextDocument = new OptionalVersionedTextDocumentIdentifier()
                            {
                                Uri = parameter.TextDocument.Uri,
                            },
                            Edits =
                                [
                                    new() {
                                        Range = new Range
                                        {
                                            Start = new Position
                                            {
                                                Line = 0,
                                                Character = 0
                                            },
                                            End = new Position
                                            {
                                                Line = 0,
                                                Character = 0
                                            }
                                        },
                                        NewText = "_"
                                    }
                                ]
                        },
                        }
        },
        Kind = CodeActionKind.QuickFix,
    };
    public static CodeAction AddTextActionWithError(Dictionary<string, TextEdit[]> changes) =>
            new()
            {
                Title = "Add Text Action - with error diagnostic",
                Edit = new WorkspaceEdit
                {
                    Changes = changes,
                },
                Diagnostics =
                [
                    new Diagnostic()
                {
                    Range = new Range
                    {
                        Start = new Position
                        {
                            Line = 0,
                            Character = 0
                        },
                        End = new Position
                        {
                            Line = 0,
                            Character = 0
                        }
                    },
                    Message = "Test Error",
                    Severity = DiagnosticSeverity.Error,
                }
                ],
                Kind = CodeActionKind.QuickFix,
            };
    public static CodeAction AddTextActionToOtherFile(TextEdit[] addTextEdit, string editFilePath) => new()
    {
        Title = "Add Text Action - Edit on different file",
        Edit = new WorkspaceEdit
        {
            DocumentChanges = new TextDocumentEdit[]
                        {
                        new()
                        {
                            TextDocument = new OptionalVersionedTextDocumentIdentifier()
                            {
                                Uri = new Uri(editFilePath),
                            },
                            Edits = addTextEdit,
                        },
                        }
        },
        Kind = CodeActionKind.QuickFix,
    };
    public static CodeAction UnresolvedAddText(CodeAction addTextAction) =>
        new()
        {
            Title = "Unresolved Add Text Action",
            Data = addTextAction,
        };
    public static CodeAction CreateFile(Uri createFileUri) => new()
    {
        Title = "Create <TheCurrentFile>.txt",
        Edit = new()
        {
            DocumentChanges = new SumType<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>[]
            {
                new CreateFile()
                {
                    Uri = createFileUri,
                    Options = new CreateFileOptions()
                    {
                        Overwrite = true,
                    }
                },
            }
        },
    };
}
