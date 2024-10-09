using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace WebAPI;

using static ActionFactory;

public partial class LanguageServer
{
    public CodeAction GetResolvedCodeAction(CodeAction parameter)
    {
        var data = parameter.Data ?? throw new NullReferenceException(nameof(parameter.Data));
        var token = JObject.FromObject(data);
        var resolvedCodeAction = token.ToObject<CodeAction>();
        return resolvedCodeAction ?? throw new InvalidOperationException("Could not resolve code action");
    }

    public object[] GetCodeActions(CodeActionParams parameter)
    {
        var documentUri = parameter.TextDocument.Uri;
        var absolutePath = documentUri.LocalPath.TrimStart('/');
        var documentFilePath = Path.GetFullPath(absolutePath);
        var documentDirectory = Path.GetDirectoryName(documentFilePath)
            ?? throw new InvalidOperationException("Could not get directory of document");
        var documentNameNoExtension = Path.GetFileNameWithoutExtension(documentFilePath);

        var createFilePath = Path.Combine(documentDirectory, documentNameNoExtension + ".txt");
        UriBuilder createFileUriBuilder = new()
        {
            Path = createFilePath,
            Host = string.Empty,
            Scheme = Uri.UriSchemeFile,
        };
        var createFileUri = createFileUriBuilder.Uri;
        var createFileAction = CreateFile(createFileUri);

        var renameNewFilePath = Path.Combine(documentDirectory, documentNameNoExtension + "_Renamed.txt");
        UriBuilder renameNewFileUriBuilder = new()
        {
            Path = renameNewFilePath,
            Host = string.Empty,
            Scheme = Uri.UriSchemeFile,
        };
        var renameNewFileUri = renameNewFileUriBuilder.Uri;
        var renameFileAction = RenameFileAction(createFileUri, renameNewFileUri);

        TextEdit[] addTextEdit =
        [
            new() {
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
                NewText = "Added text!"
            }
        ];
        var addTextAction = AddTextAction(parameter, addTextEdit);
        Dictionary<string, TextEdit[]> changes = new()
        {
            { parameter.TextDocument.Uri.AbsoluteUri, addTextEdit }
        };

        var addTextActionChangesProperty = GetAddTextActionChangesProperty(changes);
        var addUnderscoreAction = GetAddUnderscoreAction(parameter);
        var addTextActionWithError = AddTextActionWithError(changes);

        var editFilePath = Path.Combine(documentDirectory, documentNameNoExtension + "2.foo");
        var addTextActionToOtherFile = AddTextActionToOtherFile(addTextEdit, editFilePath);
        var unresolvedAddText = UnresolvedAddText(addTextAction);

        return [
            addTextAction,
            addTextActionChangesProperty,
            addTextActionWithError,
            addTextActionToOtherFile,
            unresolvedAddText,
            addUnderscoreAction,
            createFileAction,
            renameFileAction,
        ];
    }
}
