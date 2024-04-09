namespace DslCopilot.Web.Services;
public class ChatSessionIdService
{
  private string _chatSessionId = string.Empty;
  public string GetChatSessionId()
  {
    if (string.IsNullOrEmpty(_chatSessionId))
    {
      _chatSessionId = Guid.NewGuid().ToString();
    }

    return _chatSessionId;
  }
}
