using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;
using Microsoft.SemanticKernel;
using Microsoft.Toolkit.Diagnostics;

namespace DslCopilot.Web.FunctionFilters;
public class PromptBankFunctionFilter(
    ITextEmbeddingGenerator textEmbeddingService,
    IMemoryDb memory)
    : FunctionFilterBase("generateCode")
{
    private static async Task<bool> CheckAndCreateIndexAsync(
        string indexName,
        int embeddingSize,
        IMemoryDb memory,
        CancellationToken token)
    {
        var indexes = await memory.GetIndexesAsync(token);
        var hasIndex = indexes.Contains(indexName);
        if (!hasIndex)
        {
            await memory.CreateIndexAsync(indexName, embeddingSize, token);
        }
        return hasIndex;
    }

    protected override async Task OnFunctionInvokedAsync(FunctionInvokedContext context, CancellationToken token)
    {
        var input = context.Arguments["input"]?.ToString();
        Guard.IsNotNull(input, nameof(input));
        var language = context.Arguments["language"]?.ToString();
        Guard.IsNotNull(language, nameof(language));
        var result = context.Result.GetValue<string>();
        Guard.IsNotNull(result, nameof(result));

        const string indexName = "prompt-bank";
        var embedding = await textEmbeddingService.GenerateEmbeddingAsync(input, token);
        await CheckAndCreateIndexAsync(indexName, embedding.Length, memory, token);
        var hasEmbeddings = await memory.GetSimilarListAsync(indexName, input, cancellationToken: token)
            .OrderByDescending(x => x.Item2)
            .Where(x => x.Item2 > 0.5)
            .Select(x => x.Item1)
            .AnyAsync(token);
        if (!hasEmbeddings)
        {
            await memory.UpsertAsync(indexName, new()
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