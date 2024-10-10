using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Newtonsoft.Json.Linq;

namespace WebAPI.LanguageService.Targets;

internal class ConfigurableLanguageServer(
    LanguageServerFlyweight flyweight)
    : ILanguageService
    {
        private TResult Invoke<TResult>(
            [CallerMemberName]string? name = null,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(name);
            var type = flyweight.GetType();
            var flyweightMethod = type.GetMethod(name)
                ?? throw new NotImplementedException($"No method {name} found in {type.Name}");
            return flyweightMethod.Invoke(flyweight, args) is TResult covertedResult
                ? covertedResult
                : throw new InvalidCastException($"The result of the flyweight method {name} is not of type {typeof(TResult).Name}");
        }
        
        private void Invoke(
            [CallerMemberName]string? name = null,
            params object[] args)
        {
            ArgumentNullException.ThrowIfNull(name);
            var type = flyweight.GetType();
            var flyweightMethod = type.GetMethod(name)
                ?? throw new NotImplementedException($"No method {name} found in {type.Name}");
            flyweightMethod.Invoke(flyweight, args);
        }

        public void Exit() => Invoke();
        public object GetCodeActions(JToken arg) => Invoke<object>(args: arg);
        public DocumentHighlight[] GetDocumentHighlights(
            DocumentHighlightParams arg, CancellationToken token)
            => Invoke<DocumentHighlight[]>(args: [arg, token]);
        public object GetDocumentSymbols(JToken arg) => Invoke<object>(args: arg);
        public object GetFoldingRanges(JToken arg) => Invoke<object>(args: arg);
        public object GetProjectContexts(JToken arg) => Invoke<object>(args: arg);
        public object GetResolvedCodeAction(JToken arg) => Invoke<object>(args: arg);
        public InitializeResult Initialize(JToken arg) => Invoke<InitializeResult>(args: arg);
        public void Initialized(JToken arg) => Invoke(args: arg);
        public void OnDidChangeConfiguration(JToken arg) => Invoke(args: arg);
        public void OnTextDocumentChanged(JToken arg) => Invoke(args: arg);
        public void OnTextDocumentClosed(JToken arg) => Invoke(args: arg);
        public CompletionList OnTextDocumentCompletion(JToken arg)
            => Invoke<CompletionList>(args: arg);
        public object[] OnTextDocumentFindReferences(
            ReferenceParams parameter, CancellationToken token)
            => Invoke<object[]>(args: [parameter, token]);
        public Hover OnTextDocumentHover(JToken arg) => Invoke<Hover>(args: arg);
        public void OnTextDocumentOpened(JToken arg) => Invoke(args: arg);
        public SignatureHelp? OnTextDocumentSignatureHelp(JToken arg)
            => Invoke<SignatureHelp?>(args: arg);
        public WorkspaceEdit Rename(JToken arg) => Invoke<WorkspaceEdit>(args: arg);
        public object? Shutdown() => Invoke<object?>();
    }

