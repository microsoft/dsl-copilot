using Microsoft.AspNetCore.Components;

namespace DslCopilot.Web.Data
{
  public class ChatMessage
  {
    public string? Message { get; set; }
    public string? Response { get; set; }
    public string? SelectedLanguage { get; set; }
  }
}
