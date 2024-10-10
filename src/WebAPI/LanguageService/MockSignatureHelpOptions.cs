using Microsoft.VisualStudio.LanguageServer.Protocol;
using System.Runtime.Serialization;

namespace WebAPI.LSP;

[DataContract]
class MockSignatureHelpOptions : SignatureHelpOptions
{
    [DataMember(Name = "mockSignatureHelp")]
    public bool MockSignatureHelp
    {
        get;
        set;
    }
}
