using Microsoft.AspNetCore.Builder;

var app = WebApplication.Create(args);

//TODO: Add semantic kernel services as web api.
//TODO: Expose LSP (Languages Server Protocol) for semantic kernel services.
// https://microsoft.github.io/language-server-protocol/specifications/lsp/3.17/specification/

//TODO: Add OpenAPI (Swagger) documentation for semantic kernel services.

//TODO: Create VsCode Extension using the LSP api in a separate dependent branch.
//TODO: Create GitHub Copilot Extension using the LSP api in a separate dependent branch.
//TODO: Create a management portal to build and configure the copilot in a separate dependent branch.

await app.RunAsync();
