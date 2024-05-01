using Microsoft.SemanticKernel;

namespace DslCopilot.Web.FunctionFilters;
public abstract class FunctionFilterBase(string? functionName = null)
  : IFunctionFilter
{
    CancellationTokenSource _cts = new();
    private bool FilterFunction(FunctionFilterContext context)
        => functionName != null && context.Function.Name != functionName;

    public void OnFunctionInvoked(FunctionInvokedContext context)
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
    protected virtual Task OnFunctionInvokedAsync(FunctionInvokedContext context, CancellationToken token) => Task.CompletedTask;

    public void OnFunctionInvoking(FunctionInvokingContext context)
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
  protected virtual Task OnFunctionInvokingAsync(FunctionInvokingContext context, CancellationToken token) => Task.CompletedTask;
}
