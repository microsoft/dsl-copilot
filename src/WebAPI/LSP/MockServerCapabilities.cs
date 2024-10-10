using Microsoft.VisualStudio.LanguageServer.Protocol;
using System.Runtime.Serialization;

namespace WebAPI.LSP;

[DataContract]
class MockServerCapabilities : VSServerCapabilities
{
    [DataMember(Name = "mock")]
    public bool Mock
    {
        get;
        set;
    }
}
