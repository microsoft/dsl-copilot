using DslCopilot.Web.KernelHelpers;
using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using DslCopilot.Web.Components;
using DslCopilot.SampleGrammar;
using DslCopilot.Core.Agents.CodeValidator;
using DslCopilot.ClassroomGrammar;
using Antlr4.Runtime;

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
builder.Services.GenerateAntlrParser("sampleDSL",
  stream => new SampleDSLLexer(stream),
  stream => new SampleDSLParser(stream),
  parser => parser.program());

builder.Services.GenerateAntlrParser("classroom",
  stream => new ClassroomLexer(stream),
  stream => new ClassroomParser(stream),
  parser => parser.program());

var aiOptions = builder.Configuration
    .SetBasePath("/")
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .AddUserSecrets<AzureOpenAIOptions>(optional: true)
    .Build()
    .GetSection("AzureOpenAI")
    .Get<AzureOpenAIOptions>()!;

var languageBlobServiceOptions = builder.Configuration
    .GetSection("LanguageBlobService")
    .Get<LanguageBlobServiceOptions>()!;

builder.Services.AddKernelWithCodeGenFilters(
  consoleService,
  chatSessionService,
  aiOptions,
  languageBlobServiceOptions,
  new CodeValidationRetrievalPluginOptions(
   //CodeValidationRetrievalPlugin.DefaultParsers
    new Dictionary<string, Func<string, (Parser parser, ParserRuleContext rule, ErrorListener listener)>>()
    {
      { "classroom", input => {
          var charStream = new AntlrInputStream(input);
          ClassroomLexer classroomLexer = new (charStream);
          var tokenStream = new CommonTokenStream(classroomLexer);
          ClassroomParser classroomParser = new(tokenStream);
          ErrorListener errorListener = new();

          classroomParser.RemoveErrorListeners();
          classroomParser.AddErrorListener(errorListener);

          return (parser: classroomParser, rule: classroomParser.program(), listener: errorListener); 
        } 
      }
    }

  ));

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
