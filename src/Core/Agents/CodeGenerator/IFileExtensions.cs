using Microsoft.Extensions.FileProviders;

namespace DslCopilot.Core.Plugins;

public static class IFileExtensions
{
  public static async Task<string> ReadAllAsync(this IFileInfo file, CancellationToken cancellationToken)
  {
    await using var stream = file.CreateReadStream();
    using var reader = new StreamReader(stream);
    return await reader.ReadToEndAsync(cancellationToken)
        .ConfigureAwait(false);
  }
}
