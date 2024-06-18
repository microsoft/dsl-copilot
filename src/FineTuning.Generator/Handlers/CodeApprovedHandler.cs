using System.Text.Json;
using DSL.FineTuning.Pipeline.Models;
using MediatR;

namespace DSL.FineTuning.Pipeline.Handlers
{
    internal class CodeApprovedHandler : INotificationHandler<PromptGeneratedCode>
    {
        private readonly GlobalSettings _settings;

        public CodeApprovedHandler(GlobalSettings settings) 
        {
            _settings = settings;
        }

        public Task Handle(PromptGeneratedCode request, CancellationToken cancellationToken)
        {
            FineTuningData fineTuningData = new(
                    new()
                    {
                        new(
                            "system",
                            request.SystemPrompt
                        ),
                        new(
                            "user",
                            request.Prompt
                        ),
                        new AssistantMessage(
                            "assistant",
                            request.Code,
                            1
                        )
                    }
                );

            using (StreamWriter writer = File.AppendText(_settings.TrainingDataLocation))
            {
                string tuningData = JsonSerializer.Serialize(fineTuningData);
                writer.WriteLine(tuningData);
            }

            Console.WriteLine("Line appended successfully.");
            return Unit.Task;
        }
    }

    internal record FineTuningData(List<Message> messages);

    internal record Message(string role, string content);

    internal record AssistantMessage(string role, string content, int weight)
        : Message(role, content);
}
