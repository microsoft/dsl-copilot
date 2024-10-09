using System.Diagnostics.Contracts;

namespace WebAPI;

public partial class LanguageServer
{
    public event EventHandler OnInitialized = delegate { };

    private void OnTargetInitialized(object? sender, EventArgs e)
        => OnInitialized?.Invoke(this, EventArgs.Empty);
        
    private void OnTargetInitializeCompletion(object? sender, EventArgs e)
    {
        void Log(object? arg)
        {
            Contract.Assert(arg != null);
            LogMessage(arg);
        }
        //TODO: Why do this at all?
        Timer timer = new(Log, null, 0, 5 * 1000);
    }
}
