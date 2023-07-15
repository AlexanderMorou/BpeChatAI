using Newtonsoft.Json;

namespace BpeChatAI.Chat;

/// <summary>Represents the usage data of a completion.</summary>
public class CompletionUsage
{
    /// <summary><para>Gets or sets the number of tokens in the prompt.</para>
    /// <para>Serializes as "prompt_tokens".</para></summary>
    [JsonProperty("prompt_tokens")]
    public int PromptTokens { get; set; }

    /// <summary><para>Gets or sets the number of tokens in the completion.</para>
    /// <para>Serializes as "completion_tokens".</para></summary>
    [JsonProperty("completion_tokens")]
    public int CompletionTokens { get; set; }

    /// <summary><para>Gets or sets the total number of tokens.</para>
    /// <para>Serializes as "total_tokens".</para>
    /// </summary>
    [JsonProperty("total_tokens")]
    public int TotalTokens { get; set; }
}
