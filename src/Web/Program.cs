using Antlr4.Runtime;
using DslCopilot.SampleGrammar;
using DslCopilot.Core.Plugins;
using DslCopilot.ClassroomGrammar;
using DslCopilot.Web.KernelHelpers;
using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using DslCopilot.Web.Components;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;

// Add services to the container.
services.AddRazorComponents()
    .AddInteractiveServerComponents();

services.AddOutputCache();

services.Configure<AzureOpenAIOptions>(
  configuration.GetSection("AzureOpenAI"));
services.Configure<LanguageBlobServiceOptions>(
  configuration.GetSection("LanguageBlobService"));

services.AddSingleton<LanguageService>();

// We need an instances of ChatSessionService and ConsoleService for the Kernel FunctionFilters to use.
ChatSessionService chatSessionService = new();
ConsoleService consoleService = new();
services.AddSingleton(_ => chatSessionService);
services.AddSingleton(_ => consoleService);

// Chat history should be scoped, since we want one per user session.
services.AddScoped<DslAIService>();
services.AddScoped<ChatSessionIdService>();
services.GenerateAntlrParser("sampleDSL",
  stream => new SampleDSLLexer(stream),
  stream => new SampleDSLParser(stream),
  parser => parser.program());

services.GenerateAntlrParser("classroom",
  stream => new ClassroomLexer(stream),
  stream => new ClassroomParser(stream),
  parser => parser.program());

var aiOptions = configuration
    .SetBasePath("/")
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<AzureOpenAIOptions>(optional: true)
    .Build()
    .GetSection("AzureOpenAI")
    .Get<AzureOpenAIOptions>()!;

var languageBlobServiceOptions = configuration
    .GetSection("LanguageBlobService")
    .Get<LanguageBlobServiceOptions>()!;

services.AddKernelWithCodeGenFilters(
  consoleService,
  chatSessionService,
  aiOptions,
  languageBlobServiceOptions,
  new(new Dictionary<string, Func<string, AntlrConfigOptions>>
  {
    { "classroom", input =>
    {
        AntlrInputStream charStream = new(input);
        ClassroomLexer classroomLexer = new(charStream);
        CommonTokenStream tokenStream = new(classroomLexer);
        ClassroomParser classroomParser = new(tokenStream);
        ErrorListener errorListener = new();

        classroomParser.RemoveErrorListeners();
        classroomParser.AddErrorListener(errorListener);

        return new(parser: classroomParser, rule: classroomParser.program(), listener: errorListener);
      }
    }
  }));

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
