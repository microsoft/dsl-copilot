namespace DslCopilot.Web.Services;
public class ConsoleService
{
  private readonly Dictionary<string, Action<string>> _registeredConsoles = [];

  public void RegisterConsole(string sessionId, Action<string> consoleDelegate)
    => _registeredConsoles[sessionId] = consoleDelegate;

  public void RemoveConsole(string sessionId)
    => _registeredConsoles.Remove(sessionId);

  public void WriteToConsole(string sessionId, string message)
  {
    if (_registeredConsoles.TryGetValue(sessionId, out var value))
    {
      value(message);
    }
  }
}
