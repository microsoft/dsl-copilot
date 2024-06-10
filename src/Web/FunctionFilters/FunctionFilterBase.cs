using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;
public abstract class FunctionFilterBase(string? functionName = null)
  : IFunctionInvocationFilter
{
    private readonly CancellationTokenSource _cts = new();
    private bool FilterFunction(FunctionInvocationContext context)
        => functionName != null && context.Function.Name != functionName;

    public void OnFunctionInvoked(FunctionInvocationContext context)
    {
        if (FilterFunction(context))
        {
            return;
        }
        OnFunctionInvokedAsync(context, _cts.Token)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();;
    }
    protected virtual Task OnFunctionInvokedAsync(FunctionInvocationContext context, CancellationToken token) => Task.CompletedTask;

    public void OnFunctionInvoking(FunctionInvocationContext context)
    {
        if (FilterFunction(context))
        {
            return;
        }
        OnFunctionInvokingAsync(context, _cts.Token)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }
  protected virtual Task OnFunctionInvokingAsync(FunctionInvocationContext context, CancellationToken token) => Task.CompletedTask;
  public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
  {
        if (FilterFunction(context))
        {
            await next(context).ConfigureAwait(false);
            return;
        }
        await OnFunctionInvokingAsync(context, _cts.Token)
            .ConfigureAwait(false);
        await next(context).ConfigureAwait(false);
        await OnFunctionInvokedAsync(context, _cts.Token)
            .ConfigureAwait(false);
  }
}
