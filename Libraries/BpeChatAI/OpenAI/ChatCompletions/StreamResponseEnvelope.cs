using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.ChatCompletions;

/// <summary>Represents a streaming completion response envelope.</summary>
internal class StreamResponseEnvelope
{
    /// <summary><para>Gets or sets the data of the streaming completion response.</para>
    /// <para>Serializes as "data".</para></summary>
    [JsonProperty("data")]
    public StreamResponse? Data { get; set; }
}
