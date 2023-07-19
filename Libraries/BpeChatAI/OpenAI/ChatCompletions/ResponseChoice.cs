using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.ChatCompletions;
/// <summary>Represents a choice in a completion</summary>
public class ResponseChoice
{
    /// <summary><para>Gets or sets the text of the choice.</para>
    /// <para>Serializes as "text".</para>
    /// </summary>
    [JsonProperty("text")]
    public string? Text { get; set; }

    /// <summary><para>Gets or sets the index of the choice.</para>
    /// <para>Serializes as "index".</para></summary>
    [JsonProperty("index")]
    public int Index { get; set; }

    /// <summary><para>Gets or sets the log probabilities of the choice.</para>
    /// <para>Serializes as "logprobs".</para></summary>
    [JsonProperty("logprobs")]
    public int? Logprobs { get; set; }

    /// <summary><para>Gets or sets the reason for finishing the choice.</para>
    /// <para>Serializes as "finish_reason".</para></summary>
    [JsonProperty("finish_reason")]
    public string? FinishReason { get; set; }

    /// <summary><para>Gets or sets the completion message.</para>
    /// <para>Serializes as "message".</para></summary>
    [JsonProperty("message")]
    public Message? Message { get; set; }
}
