using Newtonsoft.Json;

namespace BpeChatAI.Moderation;

/// <summary>Represents a moderation input.</summary>
public class ModerationInput
{
    /// <summary>Initializes a new instance of the <see cref="ModerationInput"/> class.</summary>
    public ModerationInput(string input)
        => Input = input;
    /// <summary><para>Gets or sets the <see cref="string"/> value denoting the input to moderate.</para>
    /// <para>Serializes as "input".</para></summary>
    [JsonProperty("input")]
    public string Input { get; set; }
    /// <summary><para>Gets or sets the <see cref="string"/> value denoting the model to use for moderation.</para>
    /// <para>Serializes as "model".</para>
    /// <para>Defaults to <see langword="null"/>. When it is omitted with <see langword="null"/>,
    /// the API endpoint will interpret it as "text-moderation-latest".</para></summary>
    [JsonProperty("model", NullValueHandling = NullValueHandling.Ignore)]
    public string? Model { get; set; }
}
