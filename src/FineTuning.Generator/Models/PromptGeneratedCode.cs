using MediatR;

namespace DSL.FineTuning.Pipeline.Models
{
    internal record PromptGeneratedCode(
            string SystemPrompt,
            string AuthorName,
            string AutherRole,
            string Prompt,
            string Code
       ) : INotification;
}
