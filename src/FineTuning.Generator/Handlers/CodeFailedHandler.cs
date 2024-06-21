using DSL.FineTuning.Pipeline.Models;
using DslCopilot.FineTuning.Generator.Models;
using MediatR;

namespace DslCopilot.FineTuning.Generator.Handlers
{
    internal class CodeFailedHandler : INotificationHandler<PromptCodeFailure>
    {
        public Task Handle(PromptCodeFailure notification, CancellationToken cancellationToken)
        {
            Console.WriteLine("There was a validation error and the code did not pass successfully.");
            Console.WriteLine($"Prompt: {notification.Prompt}");
            Console.WriteLine("Code:");
            Console.WriteLine(notification.Code);

            return Unit.Task;
        }
    }
}
