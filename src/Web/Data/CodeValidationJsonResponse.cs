using System.Text.Json.Serialization;

namespace DslCopilot.Web.Data
{
    public record CodeValidationJsonResponse(
        [property: JsonPropertyName("isValid")] bool IsValid,
        [property: JsonPropertyName("errors")] string[] Errors);
}