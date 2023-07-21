using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.Moderation;

/// <summary><para>Represents a moderation result.</para></summary>
public class Response
    : ApiResponseWithSuccessInformation
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
    public List<Result>? Results { get; set; }
    /// <summary><para>Gets whether any of the results were flagged.</para>
    /// <para>Convenience property for checking if any of the results were flagged and is not
    /// serialized.</para></summary>
    [JsonIgnore]
    public bool IsFlagged
        => Results?.Any(x => x.Flagged) ?? false;

}
