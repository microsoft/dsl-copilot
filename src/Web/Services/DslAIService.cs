using DslCopilot.Core.Agents;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DslCopilot.Web.Services;
public class DslAIService(
  Kernel kernel,
  ChatSessionIdService chatSessionIdService,
  ChatSessionService chatSessionService)
{
  public async Task<string> AskAI(
    string userMessage,
    string language,
    string antlrDef,
    CancellationToken cancellationToken)
  {
    var chatSessionId = chatSessionIdService.GetChatSessionId();
    var chatHistory = chatSessionService.GetChatSession(chatSessionId);

    if (chatHistory.Count == 0)
    {
      chatHistory.AddSystemMessage($"You are an assistant for generating code that conforms to a given grammar.");
    }

    var response = await AgentChat(
        userMessage,
        language,
        antlrDef,
        cancellationToken)
      .ConfigureAwait(false);
    chatHistory.AddUserMessage(userMessage);
    chatHistory.AddAssistantMessage(response);

    return response;
  }

  private async Task<string> AgentChat(string message,
    string language, string antlrDef,
    CancellationToken cancellationToken)
  {
    var agentFactory = new AgentFactory(kernel);
    var codeGenAgent = agentFactory.CreateCodeGenerator();
    var agentChat = new AgentGroupChat(
      codeGenAgent,
      agentFactory.CreateCodeValidator(),
      agentFactory.CreateCodeCustodian())
    {
      ExecutionSettings = new()
      {
        TerminationStrategy =
          {
            MaximumIterations = 5
          }
      }
    };
    var systemMessage = new ChatMessageContent(AuthorRole.System,
      @$"I have a language called '{language}' that is defined by the following grammar:
        {antlrDef}
        "
      );
    agentChat.AddChatMessage(systemMessage);
    var chatMessage = new ChatMessageContent(AuthorRole.User, message);
    agentChat.AddChatMessage(chatMessage);

    var messages = agentChat.GetChatMessagesAsync(codeGenAgent, cancellationToken);
    var lastMessage = await messages.LastAsync(cancellationToken).ConfigureAwait(false);
    return lastMessage.InnerContent?.ToString() ?? "No response";
  }
}