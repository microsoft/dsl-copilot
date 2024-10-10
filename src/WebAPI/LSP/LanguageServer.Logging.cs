using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LSP;

public partial class LanguageServer
{
    public void LogMessage(object arg)
        => LogMessage(arg, MessageType.Info);

    public void LogMessage(object arg, MessageType messageType)
        => LogMessage(arg, "testing " + counter++, messageType);

    public void LogMessage(object arg, string message, MessageType messageType)
        => _ = SendMethodNotificationAsync(Methods.WindowLogMessage, new()
        {
            Message = message,
            MessageType = messageType
        });
}
