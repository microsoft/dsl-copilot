using DslCopilot.Web.Options;
using DslCopilot.Web.Services;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;

public class DebuggingPromptFilter
  (ILogger<DebuggingPromptFilter> logger) : IPromptFilter
{
  public void OnPromptRendered(PromptRenderedContext context)
  {
    logger.LogDebug($"Prompt rendered: {context.RenderedPrompt}");
  }
  public void OnPromptRendering(PromptRenderingContext context) { }
}
