using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LSP;

public class SymbolInfo(string name, SymbolKind kind, string container)
{
    public string Name
    {
        get;
        set;
    } = name;

    public SymbolKind Kind
    {
        get;
        set;
    } = kind;

    public string Container
    {
        get;
        set;
    } = container;
}
