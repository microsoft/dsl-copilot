using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WebAPI;

public partial class LanguageServer : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    private void NotifyPropertyChanged(
        [CallerMemberName]string? propertyName = default)
        => PropertyChanged?.Invoke(this, new(propertyName));
}
