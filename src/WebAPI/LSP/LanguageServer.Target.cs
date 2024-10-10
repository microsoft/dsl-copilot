namespace WebAPI.LSP;

public partial class LanguageServer
{
    public bool IsIncomplete
    {
        get => target.IsIncomplete;
        set
        {
            if (target.IsIncomplete != value)
            {
                target.IsIncomplete = value;
                NotifyPropertyChanged(nameof(IsIncomplete));
            }
        }
    }

    public bool CompletionServerError
    {
        get => target.CompletionServerError;
        set => target.CompletionServerError = value;
    }

    public bool ItemCommitCharacters
    {
        get => target.ItemCommitCharacters;
        set => target.ItemCommitCharacters = value;
    }
}
