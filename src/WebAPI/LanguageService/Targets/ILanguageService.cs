using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;
using StreamJsonRpc;

namespace WebAPI.LanguageService.Targets;

public interface ILanguageService
{
    [JsonRpcMethod(Methods.InitializeName)]
    InitializeResult Initialize(JToken arg);
    
    [JsonRpcMethod(Methods.InitializedName)]
    void Initialized(JToken arg);
    
    [JsonRpcMethod(Methods.TextDocumentDidOpenName)]
    public void OnTextDocumentOpened(JToken arg);
    
    [JsonRpcMethod(Methods.TextDocumentHoverName)]
    public Hover OnTextDocumentHover(JToken arg);

    [JsonRpcMethod(Methods.TextDocumentDidChangeName)]
    public void OnTextDocumentChanged(JToken arg);
    
    [JsonRpcMethod(Methods.TextDocumentDidCloseName)]
    public void OnTextDocumentClosed(JToken arg);
    
    [JsonRpcMethod(Methods.TextDocumentReferencesName,
        UseSingleObjectParameterDeserialization = true)]
    public object[] OnTextDocumentFindReferences(
        ReferenceParams parameter, CancellationToken token);

    [JsonRpcMethod(Methods.TextDocumentCodeActionName)]
    public object GetCodeActions(JToken arg);

    [JsonRpcMethod(Methods.CodeActionResolveName)]
    public object GetResolvedCodeAction(JToken arg);

    [JsonRpcMethod(Methods.TextDocumentCompletionName)]
    public CompletionList OnTextDocumentCompletion(JToken arg);
    
    [JsonRpcMethod(Methods.TextDocumentSignatureHelpName)]
    public SignatureHelp? OnTextDocumentSignatureHelp(JToken arg);

    [JsonRpcMethod(Methods.WorkspaceDidChangeConfigurationName)]
    public void OnDidChangeConfiguration(JToken arg);

    [JsonRpcMethod(Methods.ShutdownName)]
    public object? Shutdown();

    [JsonRpcMethod(Methods.ExitName)]
    public void Exit();
    
    [JsonRpcMethod(Methods.TextDocumentRenameName)]
    public WorkspaceEdit Rename(JToken arg);

    [JsonRpcMethod(Methods.TextDocumentFoldingRangeName)]
    public object GetFoldingRanges(JToken arg);
    
    [JsonRpcMethod(VSMethods.GetProjectContextsName)]
    public object GetProjectContexts(JToken arg);

    [JsonRpcMethod(Methods.TextDocumentDocumentSymbolName)]
    public object GetDocumentSymbols(JToken arg);

    [JsonRpcMethod(Methods.TextDocumentDocumentHighlightName,
        UseSingleObjectParameterDeserialization = true)]
    public DocumentHighlight[] GetDocumentHighlights(
        DocumentHighlightParams arg, CancellationToken token);
}
