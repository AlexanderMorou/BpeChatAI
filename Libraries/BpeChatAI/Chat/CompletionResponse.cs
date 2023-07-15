using Newtonsoft.Json;

namespace BpeChatAI.Chat;
/// <summary>Represents a completion response.</summary>
public class CompletionResponse
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

    /// <summary><para>Gets or sets the list of choices in the response.</para>
    /// <para>Serializes as "choices".</para></summary>
    [JsonProperty("choices")]
    public List<CompletionChoice>? Choices { get; set; }

    /// <summary><para>Gets or sets the usage data of the response.</para>
    /// <para>Serializes as "usage".</para></summary>
    [JsonProperty("usage")]
    public CompletionUsage? Usage { get; set; }

    /// <summary>Gets or sets whether the response was successful or not.</summary>
    [JsonIgnore()]
    public bool IsSuccess { get; set; }

    /// <summary>Gets or sets the error message if there was an error in the response.</summary>
    [JsonIgnore()]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }
}
