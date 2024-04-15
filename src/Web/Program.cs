using DslCopilot.Web.KernelHelpers;
using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using DslCopilot.Web;
using DslCopilot.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOutputCache();


builder.Services.Configure<AzureOpenAIOptions>(
  builder.Configuration.GetSection("AzureOpenAI"));
builder.Services.Configure<LanguageBlobServiceOptions>(
   builder.Configuration.GetSection("LanguageBlobService"));

builder.Services.AddSingleton<LanguageService>();

// We need an instances of ChatSessionService and ConsoleService for the Kernel FunctionFilters to use.
ChatSessionService chatSessionService = new();
ConsoleService consoleService = new();
builder.Services.AddSingleton(_ =>
{
  return chatSessionService;
});
builder.Services.AddSingleton(_ =>
{
  return consoleService;
});

// Chat history should be scoped, since we want one per user session.
builder.Services.AddScoped<DslAIService>();
builder.Services.AddScoped<ChatSessionIdService>();

builder.Services.AddKernelWithCodeGenFilters(consoleService, chatSessionService, builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAIOptions>());

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseOutputCache();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
