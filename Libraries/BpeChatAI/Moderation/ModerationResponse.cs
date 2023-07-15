using Newtonsoft.Json;

namespace BpeChatAI.Moderation;

/// <summary><para>Represents a moderation result.</para></summary>
public class ModerationResponse
{
    /// <summary><para>Gets or sets the id of the moderation result.</para></summary>
    [JsonProperty("id")]
    public string? Id { get; set; }
    /// <summary><para>Gets or sets the model of the moderation result.</para></summary>
    [JsonProperty("model")]
    public string? Model { get; set; }
    /// <summary><para>Gets or sets the list of 
    /// of the moderation result elements.</para>
    /// <para>Serializes as "results".</para></summary>
    [JsonProperty("results")]
    public List<ModerationResult>? Results { get; set; }
    /// <summary><para>Gets or sets whether the moderation request was successful.</para>
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess { get; set; }
    /// <summary>Returns or sets the <see cref="string"/> value denoting the error message
    /// when the streaming completion response had an error.</summary>
    [JsonIgnore]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }

}
