using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;

namespace WebAPI;

public partial class LanguageServer
{
    private readonly JsonRpc rpc;

    public event EventHandler Disconnected = delegate { };
    public JsonRpc Rpc => rpc;

    public void WaitForExit() => disconnectEvent.WaitOne();

    public void Exit()
    {
        disconnectEvent.Set();
        Disconnected?.Invoke(this, new EventArgs());
    }

    private void OnRpcDisconnected(object? sender, JsonRpcDisconnectedEventArgs e)
        => Exit();

    private Task SendMethodNotificationAsync<TIn>(LspNotification<TIn> method, TIn param)
        => rpc.NotifyWithParameterObjectAsync(method.Name, param);

    private Task<TOut> SendMethodRequestAsync<TIn, TOut>(LspRequest<TIn, TOut> method, TIn param)
        => rpc.InvokeWithParameterObjectAsync<TOut>(method.Name, param);
}
