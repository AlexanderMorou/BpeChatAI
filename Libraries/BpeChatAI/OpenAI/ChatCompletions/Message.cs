using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using ModerationParameters = BpeChatAI.OpenAI.Moderation.Parameters;
namespace BpeChatAI.OpenAI.ChatCompletions;

/// <summary>Represents a completion message.</summary>
public class Message
{
    /// <summary><para>Gets or sets the content of the message</para>
    /// <para>Serializes as "content".</para></summary>
    [JsonProperty("content")]
    public string? Content { get; set; }

    /// <summary><para>Gets or sets the role of the message</para>
    /// <para>Serializes as "role".</para></summary>
    [JsonProperty("role")]
    public string? Role { get; set; }

    /// <summary><para>Gets or sets the name of the message</para>
    /// <para>Serializes as "name".</para></summary>
    [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
    public string? Name { get; set; }

    /// <summary><para>Gets or sets whether the <see cref="Message"/> has been moderated</para>
    /// <para>If <see langword="true"/>, the <see cref="Message"/> has been sent through the
    /// <see cref="ApiClient.ModerateAsync(ModerationParameters)"/>. Otherwise the
    /// <see cref="Message"/> has not yet been sent through the moderation API.</para></summary>
    [JsonIgnore]
    public bool Moderated { get; set; } = false;
}
