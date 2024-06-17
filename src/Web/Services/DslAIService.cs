﻿using DslCopilot.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DslCopilot.Web.Services;
public class DslAIService(
  IKernelBuilder kernelBuilder,
  ChatSessionIdService chatSessionIdService,
  ChatSessionService chatSessionService)
{
  public async Task<string> AskAI(
    string userMessage,
    string language,
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
        cancellationToken)
      .ConfigureAwait(false);
    chatHistory.AddUserMessage(userMessage);
    chatHistory.AddAssistantMessage(response);

    return response;
  }

  private async Task<string> AgentChat(
    string message,
    string language,
    CancellationToken cancellationToken)
  {
    var agentFactory = new AgentFactory(kernelBuilder);
    var codeGenAgent = agentFactory.CreateCodeGenerator();
    var validationAgent = agentFactory.CreateCodeValidator();
    var agentChat = new AgentGroupChat(codeGenAgent, validationAgent)
    {
      ExecutionSettings = new()
      {
        TerminationStrategy =
        {
          MaximumIterations = 5,
        },
      }
    };
    // Add a message to the agent chat to set the context.
    agentChat.AddChatMessage(new(AuthorRole.User,
      $"Generate code for the '{language}' coding language."));
    // Add the user message to the agent chat.
    agentChat.AddChatMessage(new(AuthorRole.User, message));
    var messages = agentChat.InvokeAsync(codeGenAgent, cancellationToken);
    var lastMessage = await messages.LastAsync(cancellationToken).ConfigureAwait(false);
    return lastMessage.Content?.ToString() ?? "No response";
  }
}
