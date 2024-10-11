using StreamJsonRpc;
using WebAPI.LanguageService.Targets;
using WebAPI.LSP;

var appBuilder = WebApplication.CreateBuilder(args);
appBuilder.Services.AddSingleton<LanguageServer>();
appBuilder.Services.AddSingleton(x => new LanguageServerBuilder()
    // .With(config => config with { }));
    );
appBuilder.Services.AddSingleton(x => x.GetRequiredService<ILanguageServiceBuilder>().Build());

var app = appBuilder.Build();
//TODO: Add semantic kernel services as web api.
//TODO: Expose LSP (Languages Server Protocol) for semantic kernel services.
// https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/

//TODO: Add OpenAPI (Swagger) documentation for semantic kernel services.

//TODO: Create VsCode Extension using the LSP api in a separate dependent branch.
//TODO: Create GitHub Copilot Extension using the LSP api in a separate dependent branch.
//TODO: Create a management portal to build and configure the copilot in a separate dependent branch.
//StreamJsonRpc.JsonRpc.Attach<ICustomTypeProvider>();
app.MapPost("/{language}/ws", async (string language, HttpContext context) =>
{
    var sockets = context.WebSockets;
    if (sockets.IsWebSocketRequest)
    {
        var webSocket = await sockets.AcceptWebSocketAsync();
        WebSocketMessageHandler handler = new(webSocket);

        var services = context.RequestServices;
        var server = services.GetRequiredKeyedService<ILanguageService>(language);
        using JsonRpc jsonRpc = new(handler, server);
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
