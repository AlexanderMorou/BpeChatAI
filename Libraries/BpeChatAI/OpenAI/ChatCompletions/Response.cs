using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.ChatCompletions;
/// <summary>Represents a completion response.</summary>
public class Response
    : ApiResponseWithSuccessInformation
{
    /// <summary><para>Represents an empty response.</para>
    /// <para>Returns a new instance every time.</para></summary>
    public static Response EmptyResponse => new Response();
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
    public List<ResponseChoice>? Choices { get; set; }

    /// <summary><para>Gets or sets the usage data of the response.</para>
    /// <para>Serializes as "usage".</para></summary>
    [JsonProperty("usage")]
    public Usage? Usage { get; set; }
}
