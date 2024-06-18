using System.Text.Json;
using DSL.FineTuning.Pipeline.Models;
using MediatR;

namespace DSL.FineTuning.Pipeline.Handlers
{
    internal class ConsoleLogHandler : INotificationHandler<PromptGeneratedCode>
    {
        public Task Handle(PromptGeneratedCode request, CancellationToken cancellationToken)
        {
            Console.WriteLine("Line appended successfully.");
            Console.WriteLine($"Prompt: {request.Prompt}");
            Console.WriteLine("Code:");
            Console.WriteLine(request.Code);

            return Unit.Task;
        }
    }
}
