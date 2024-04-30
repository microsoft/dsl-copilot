namespace DslCopilot.Web.Data;
public class ChatMessage
{
  public string Message { get; set; } = string.Empty;
  public string Response { get; set; } = string.Empty;
  public string? SelectedLanguage { get; set; }
  public int Rating { get; set; }

  public bool IsValidResponse { get; set; }

  public bool IsInValidResponse { get; set; }

  public string CodeComments { get; set; }
}
