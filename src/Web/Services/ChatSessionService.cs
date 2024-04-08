using Microsoft.SemanticKernel.ChatCompletion;

namespace DslCopilot.Web.Services
{
  public class ChatSessionService
  {
    private readonly Dictionary<string, ChatHistory> _chatSessions = new();

    public ChatHistory GetChatSession(string sessionId)
    {
      if (!_chatSessions.ContainsKey(sessionId))
      {
        _chatSessions[sessionId] = new ChatHistory();
      }
      return _chatSessions[sessionId];
    }
  }
}
