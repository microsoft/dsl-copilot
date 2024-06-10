using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;

public class DebuggingPromptFilter
  (ILogger<DebuggingPromptFilter> logger) : IPromptRenderFilter
{
  public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
  {
    await OnPromptRendering(context).ConfigureAwait(false);
    await next(context).ConfigureAwait(false);
    await OnPromptRendered(context).ConfigureAwait(false);
  }

  public Task OnPromptRendered(PromptRenderContext context)
  {
    logger.LogDebug($"Prompt rendered: {context.RenderedPrompt}");
    return Task.CompletedTask;
  }
  public Task OnPromptRendering(PromptRenderContext context) => Task.CompletedTask;
}
