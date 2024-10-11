using Microsoft.VisualStudio.LanguageServer.Protocol;

namespace WebAPI.LanguageService.Targets;

public record NotificationParams<T>(
    LspNotification<T> Notification,
    T Value);
