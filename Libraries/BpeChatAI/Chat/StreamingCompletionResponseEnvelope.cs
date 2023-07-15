using Newtonsoft.Json;

namespace BpeChatAI.Chat;

/// <summary>Represents a streaming completion response envelope.</summary>
internal class StreamingCompletionResponseEnvelope
{
    /// <summary><para>Gets or sets the data of the streaming completion response.</para>
    /// <para>Serializes as "data".</para></summary>
    [JsonProperty("data")]
    public StreamingCompletionResponse? Data { get; set; }
}
