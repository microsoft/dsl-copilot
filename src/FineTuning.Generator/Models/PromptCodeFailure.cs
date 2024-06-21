using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace DslCopilot.FineTuning.Generator.Models
{
    internal record PromptCodeFailure(
            string Prompt,
            string Code
       ) : INotification;
}
