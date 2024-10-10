namespace WebAPI.LanguageService.Targets;

public interface ILanguageServiceBuilder
{
    ILanguageServiceBuilder With(
        Func<LanguageServerFlyweight, LanguageServerFlyweight> flyweightFactory);
    ILanguageService Build();
}
