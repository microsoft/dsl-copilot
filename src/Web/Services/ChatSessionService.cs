using Microsoft.SemanticKernel.ChatCompletion;

namespace DslCopilot.Web.Services
{
  public class ChatSessionService
  {
    private readonly Dictionary<string, ChatHistory> _chatSessions = [];

    public ChatHistory GetChatSession(string sessionId)
    {
      if (!_chatSessions.TryGetValue(sessionId, out ChatHistory? _))
      {
        _chatSessions[sessionId] = [];
      }
      return _chatSessions[sessionId];
    }
  }
}
