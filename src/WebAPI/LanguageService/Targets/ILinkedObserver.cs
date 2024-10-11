using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LanguageService.Targets;

public interface ILinkedObserver<T>
    : IDisposable
{
    protected internal IObserver<NotificationParams<T>>? Observer { get; }

    protected internal void OnNext(LspNotification<T> method, T arg)
        => Observer?.OnNext(new(method, arg));
}
