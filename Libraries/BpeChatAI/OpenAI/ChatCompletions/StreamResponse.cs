using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.ChatCompletions;
/// <summary>Represents a streaming completion response.</summary>
public class StreamResponse
{
    /// <summary><para>Gets or sets the ID of the response.</para>
    /// <para>Serializes as "id".</para></summary>
    [JsonProperty("id")]
    public string? Id { get; set; }

    /// <summary><para>Gets or sets the object type of the response.</para>
    /// <para>Serializes as "object".</para></summary>
    [JsonProperty("object")]
    public string? Object { get; set; }

    /// <summary><para>Gets or sets the timestamp of when the response was created.</para>
    /// <para>Serializes as "created".</para></summary>
    [JsonProperty("created")]
    public long Created { get; set; }

    /// <summary><para>Gets or sets the model used in the response.</para>
    /// <para>Serializes as "model".</para></summary>
    [JsonProperty("model")]
    public string? Model { get; set; }

    /// <summary><para>Gets or sets the list of stream completion choices in the response.</para>
    /// <para>Serializes as "choices".</para></summary>
    [JsonProperty("choices")]
    public List<StreamResponseChoice>? Choices { get; set; }

    /// <summary>Gets or sets whether an error occurred when making the reques, or not.</summary>
    [JsonIgnore]
    public bool ErrorOnRequest { get; set; }
    /// <summary>Gets or sets whether the streaming completion response was successful.</summary>
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
