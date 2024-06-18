using DslCopilot.Core;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

namespace DSL.FineTuning.Pipeline.Services
{
    internal class OpenAIChatService
    {
        private readonly IKernelBuilder _kernelBuilder;

        public OpenAIChatService(
            IKernelBuilder kernelBuilder
        ) {
            _kernelBuilder = kernelBuilder;
        }

        public async Task<(ChatMessageContent chatMessageContent, ChatHistoryKernelAgent primaryAgent)> AgentChat(
            string message,
            string language,
            CancellationToken cancellationToken
        ) {
            var agentFactory = new AgentFactory(_kernelBuilder);
            var codeGenAgent = agentFactory.CreateCodeGenerator();
            var validationAgent = agentFactory.CreateCodeValidator();
            var agentChat = new AgentGroupChat(codeGenAgent, validationAgent)
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

            agentChat.AddChatMessage(
                new(
                    AuthorRole.User,
                    $"Generate code for the '{language}' coding language."
                )
             );

            agentChat.AddChatMessage(new(AuthorRole.User, message));

            var messages = await agentChat.InvokeAsync(cancellationToken).ToListAsync();
            return (
                messages.Last(c => c.AuthorName == codeGenAgent.Name),
                codeGenAgent
            );
        }
    }
}
