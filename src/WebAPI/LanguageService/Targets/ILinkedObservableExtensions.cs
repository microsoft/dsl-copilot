using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LanguageService.Targets;

public static class ILinkedObservableExtensions
{
    public static void Dispose<T>(this ILinkedObserver<T> observable)
        => observable.Dispose();
    public static void OnNext<T>(this ILinkedObserver<T> observable,
        LspNotification<T> notification, T value)
        => observable.Observer?.OnNext(new(notification, value));
}
