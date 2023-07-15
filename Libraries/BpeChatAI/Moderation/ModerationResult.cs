using Newtonsoft.Json;

namespace BpeChatAI.Moderation;

/// <summary>Represents a moderation result.</summary>
public class ModerationResult
{
    /// <summary><para>Gets or sets the categories that moderate the
    /// <see cref="ModerationInput"/>.</para><para>Serializes as
    /// "categories".</para></summary>
    [JsonProperty("categories")]
    public Dictionary<string, bool>? Categories { get; set; }
    /// <summary><para>Gets or sets the category scores for the
    /// <see cref="ModerationInput"/>.</para>
    /// <para>Serializes as "category_scores".</para></summary>
    [JsonProperty("category_scores")]
    public Dictionary<string, float>? CategoryScores { get; set; }
    /// <summary><para>Gets or sets whether the <see cref="ModerationInput"/>
    /// was flagged.</para><para>Serializes as "flagged".</para></summary>
    [JsonProperty("flagged")]
    public bool Flagged { get; set; }
}
