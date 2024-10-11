namespace WebAPI.LanguageService.Targets;

public partial class LanguageServerBuilder(
    object? _flyweight = null)
    : ILanguageServiceBuilder
{
    //Note: This is to avoid capturing multiple flyweights
    private object _flyweight = _flyweight ?? new();

    /// <summary>
    /// Exposes a Copy method that passes configurations by value for extension.
    /// </summary>
    /// <param name="flyweightFactory">The current configuration</param>
    /// <returns>A new instance of a builder with the augmented configuration.</returns>
    /// <example>
    /// var service = new Builder()
    ///     .With(config => config with { Mock = true })
    ///     .Build();
    /// </example>
    public ILanguageServiceBuilder With(Func<object, object> flyweightFactory)
        => new LanguageServerBuilder(flyweightFactory(_flyweight));

    public ILanguageService Build() => throw new NotImplementedException();
        //new ConfigurableLanguageServer(_flyweight);
}

