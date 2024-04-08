using Microsoft.AspNetCore.Components;

namespace DslCopilot.Web.Data
{
  public class ChatMessage
  {
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public string? SelectedLanguage { get; set; }
  }
}
