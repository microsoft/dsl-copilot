using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.FunctionFilters;
public class PromptBankFunctionFilter(
    ITextEmbeddingGenerator textEmbeddingService,
    IMemoryDb memory, int exampleCount = 3)
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

        var similarities = memory.GetSimilarListAsync("prompt-bank", input, cancellationToken: token)
            .OrderByDescending(x => x.Item2)
            .Where(x => x.Item2 > 0.5)
            .Select(x => x.Item1)
            .Take(exampleCount);
        var hasAny = await similarities.AnyAsync(token);
        if (hasAny)
        {
            await foreach (var item in similarities)
            {
            }
        }
        else
        {
            await memory.CreateIndexAsync("prompt-bank", 10, token);
            var embedding = await textEmbeddingService.GenerateEmbeddingAsync(input, token);
            await memory.UpsertAsync("prompt-bank", new MemoryRecord
            {

                Id = Guid.NewGuid().ToString(),
                Vector = embedding,
                Tags =
                {
                    ["language"] = [language]
                },
                Payload =
                {
                    ["prompt"] = input,
                    ["response"] = result
                }
            }, token);
        }
    }
}