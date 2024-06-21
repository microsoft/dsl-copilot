using DslCopilot.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DSL.FineTuning.Pipeline.Services;
internal class OpenAIChatService(IKernelBuilder kernelBuilder)
{
    public async Task<(ChatMessageContent chatMessageContent, ChatHistoryKernelAgent primaryAgent, bool isComplete)> AgentChat(
        string message,
        string language,
        CancellationToken cancellationToken)
    {
        AgentFactory agentFactory = new(kernelBuilder);
        var codeGenAgent = agentFactory.CreateCodeGenerator();
        var validationAgent = agentFactory.CreateCodeValidator();
        AgentGroupChat agentChat = new(codeGenAgent, validationAgent)
        {
            ExecutionSettings = new()
            {
                TerminationStrategy = new ApprovalTerminationStrategy()
                {
                    Agents = [validationAgent],
                    MaximumIterations = 6,
                }
            }
        };

        agentChat.AddChatMessage(new(AuthorRole.User,
            $"Generate code for the '{language}' coding language."));
        agentChat.AddChatMessage(new(AuthorRole.User, message));

        var messages = await agentChat
            .InvokeAsync(cancellationToken)
            .ToListAsync(cancellationToken);
        return (
            messages.Last(c => c.AuthorName == codeGenAgent.Name),
            codeGenAgent,
            agentChat.IsComplete
        );
    }
}
