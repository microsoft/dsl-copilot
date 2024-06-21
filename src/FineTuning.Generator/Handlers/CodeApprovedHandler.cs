using System.Text.Json;
using System.Text.Json.Serialization;
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
                        new(
                            "assistant",
                            request.Code,
                            1
                        )
                    }
                );

            using (StreamWriter writer = File.AppendText(_settings.TrainingDataLocation))
            {
                string tuningData = JsonSerializer.Serialize(fineTuningData, new JsonSerializerOptions {
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });
                writer.WriteLine(tuningData);
            }

            return Unit.Task;
        }
    }

    internal record FineTuningData(List<Message> messages);

    internal record Message(string role, string content, int? weight = null);
}
