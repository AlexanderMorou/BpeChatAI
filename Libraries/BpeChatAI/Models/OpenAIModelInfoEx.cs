using BpeChatAI.Models;

using Newtonsoft.Json;

namespace BpeChatAI.OpenAI;
/// <summary>Represents a list of <see cref="OpenAIModelInfo"/> objects.</summary>
public class OpenAIModelInfoEx
    : OpenAIModelInfo
{
    /// <summary>Gets or sets the success status of the request.</summary>
    [JsonIgnore]
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Gets or sets the <see cref="string"/> value denoting the error message
    /// when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    [JsonIgnore]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }
}
