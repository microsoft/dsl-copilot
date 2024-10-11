namespace WebAPI.LanguageService.Targets;

public interface ILanguageServiceBuilder
{
    ILanguageServiceBuilder With(
        Func<object, object> flyweightFactory);
    ILanguageService Build();
}
