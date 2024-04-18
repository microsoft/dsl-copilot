using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.FunctionFilters;
public class PromptBankFunctionFilter(IKernelMemory memory)
    : FunctionFilterBase("generateCode")
{
    protected override async Task OnFunctionInvokedAsync(FunctionInvokedContext context, CancellationToken token)
    {
        var input = context.Arguments["input"]?.ToString();
        Guard.IsNotNull(input, nameof(input));
        var language = context.Arguments["language"]?.ToString();
        Guard.IsNotNull(language, nameof(language));
        var result = context.Result.GetValue<string>();
        Guard.IsNotNull(result, nameof(result));

        var searchResult = await memory
            .SearchAsync(input, index: "promptBank", limit: 1, minRelevance: 0.5, cancellationToken: token)
            .ConfigureAwait(false);
        if (searchResult.NoResult)
        {
            await memory
                .ImportTextAsync($"prompt > {input}{Environment.NewLine}response > {result}",
                    tags: new() { { "language", language } },
                    index: "promptBank",
                    cancellationToken: token)
                .ConfigureAwait(false);
        }
    }
}