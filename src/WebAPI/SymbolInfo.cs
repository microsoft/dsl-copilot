using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI;

public class SymbolInfo
{
    public SymbolInfo(string name, SymbolKind kind, string container)
    {
        Name = name;
        Kind = kind;
        Container = container;
    }

    public string Name
    {
        get;
        set;
    }

    public SymbolKind Kind
    {
        get;
        set;
    }

    public string Container
    {
        get;
        set;
    }
}
