using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI;
using Microsoft.KernelMemory.MemoryStorage;

namespace DslCopilot.Web.Services;

public class PromptBankService(ITextEmbeddingGenerator generator, IMemoryDb memory)
{
  public IAsyncEnumerable<MemoryRecord> Search(string index, string input, int count, double tolerance, CancellationToken cancellationToken)
    => memory.GetSimilarListAsync(index, input, cancellationToken: cancellationToken)
        .OrderByDescending(x => x.Item2)
        .Where(x => x.Item2 > tolerance)
        .Select(x => x.Item1)
        .Take(count);

  public async Task TryCreateIndex(string index, int size, CancellationToken cancellationToken)
    {
        var indices = await memory.GetIndexesAsync(cancellationToken);
        if (!indices.Contains(index))
        {
            await memory.CreateIndexAsync(index, size, cancellationToken);
        }
    }

    public async Task Upsert(string index, string prompt, string response, TagCollection tags, CancellationToken cancellationToken)
    {
        var embedding = await generator.GenerateEmbeddingAsync(prompt, cancellationToken);
        await TryCreateIndex(index, embedding.Length, cancellationToken);
        await memory.UpsertAsync(index, new()
        {
            Id = Guid.NewGuid().ToString(),
            Vector = embedding,
            Tags = tags,
            Payload =
            {
                ["prompt"] = prompt,
                ["response"] = response
            }
        }, cancellationToken);
    }
}