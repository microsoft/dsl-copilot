using Nerdbank.Streams;
using StreamJsonRpc;
using WebAPI;

var app = WebApplication.Create(args);
//TODO: Add semantic kernel services as web api.
//TODO: Expose LSP (Languages Server Protocol) for semantic kernel services.
// https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/

//TODO: Add OpenAPI (Swagger) documentation for semantic kernel services.

//TODO: Create VsCode Extension using the LSP api in a separate dependent branch.
//TODO: Create GitHub Copilot Extension using the LSP api in a separate dependent branch.
//TODO: Create a management portal to build and configure the copilot in a separate dependent branch.
//StreamJsonRpc.JsonRpc.Attach<ICustomTypeProvider>();
app.MapPost("/ws", async (HttpContext context) =>
{
    var sockets = context.WebSockets;
    if (sockets.IsWebSocketRequest)
    {
        var webSocket = await sockets.AcceptWebSocketAsync();
        WebSocketMessageHandler handler = new(webSocket);

        var server = new LanguageServer(
            webSocket.UsePipeReader().AsStream(),
            webSocket.UsePipeWriter().AsStream());
        using var jsonRpc = server.Rpc;
        jsonRpc.CancelLocallyInvokedMethodsWhenConnectionIsClosed = true;
        jsonRpc.StartListening();
        await Task.WhenAll(
            jsonRpc.Completion,
            jsonRpc.DispatchCompletion);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});
await app.RunAsync();
