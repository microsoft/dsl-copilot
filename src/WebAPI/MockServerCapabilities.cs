using Microsoft.VisualStudio.LanguageServer.Protocol;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace WebAPI;

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
public static class LogUtils
    {
        public static TraceSource CreateTraceSource()
        {
            var traceSource = new TraceSource("MockLanguageExtension", SourceLevels.Verbose | SourceLevels.ActivityTracing);

            var traceFileDirectoryPath = Path.Combine(Path.GetTempPath(), "VisualStudio", "LSP");
            var logFilePath = Path.Combine(traceFileDirectoryPath, "MockLog.svclog");
            var traceListener = new XmlWriterTraceListener(logFilePath);

            traceSource.Listeners.Add(traceListener);

            Trace.AutoFlush = true;

            return traceSource;
        }

    }