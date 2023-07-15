using Newtonsoft.Json;

namespace BpeChatAI.Chat;
/// <summary>Represents a completion choice that was streamed.</summary>
public class StreamCompletionChoice
{
    /// <summary><para>Gets or sets the index of the choice.</para>
    /// <para>Serializes as "index".</para></summary>
    [JsonProperty("index")]
    public int Index { get; set; }

    /// <summary><para>Gets or sets the reason for finishing the choice.</para>
    /// <para>Serializes as "finish_reason".</para></summary>
    [JsonProperty("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary><para>Gets or sets the delta of the completion choice.</para>
    /// <para>Serializes as "delta".</para></summary>
    [JsonProperty("delta")]
    public CompletionMessage? Delta { get; set; }
}
