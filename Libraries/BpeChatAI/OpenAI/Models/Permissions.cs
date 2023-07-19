using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.Models;

/// <summary>Represents model permissions.</summary>
public class Permissions
{
    /// <summary><para>Gets or sets the Id of the model.</para>
    /// <para>Serializes as "id".</para></summary>
    [JsonProperty("id")]
    public string? Id { get; set; }
    /// <summary><para>Gets or sets the type of object represented by the model.</para>
    /// <para>Serializes as "object".</para></summary>
    [JsonProperty("object")]
    public string? Object { get; set; }
    /// <summary><para>Gets or sets the UNIX timestamp indicating time of creation.</para>
    /// <para>Serializes as "created".</para></summary>
    [JsonProperty("created")]
    public long Created { get; set; }
    /// <summary><para>
    /// Gets or sets whether allows creating engines on the model.
    /// </para><para>Serializes as "allow_create_engine".</para></summary>
    [JsonProperty("allow_create_engine")]
    public bool AllowCreateEngine { get; set; }
    /// <summary><para>Gets or sets whether sampling is allowed on the model.</para>
    /// <para>Serializes as "allow_sampling".</para></summary>
    [JsonProperty("allow_sampling")]
    public bool AllowSampling { get; set; }
    /// <summary>
    /// <para>Gets or sets whether log probabilities are allowed on the model.
    /// </para><para>Serializes as "allow_logprobs".</para></summary>
    [JsonProperty("allow_logprobs")]
    public bool AllowLogprobs { get; set; }
    /// <summary><para>Gets or sets whether the model allows search indices.</para>
    /// <para>Serializes as "allow_search_indices".</para></summary>
    [JsonProperty("allow_search_indices")]
    public bool AllowSearchIndices { get; set; }
    /// <summary>
    /// <para>Gets or sets whether the model allows view.</para>
    /// <para>Serializes as "allow_view".</para></summary>
    [JsonProperty("allow_view")]
    public bool AllowView { get; set; }
    /// <summary><para>Gets or sets whether the model allows fine tuning.</para>
    /// <para>Serializes as "allow_fine_tuning".</para></summary>
    [JsonProperty("allow_fine_tuning")]
    public bool AllowFineTuning { get; set; }
    /// <summary><para>Gets or sets the owning organization.</para>
    /// <para>Serializes as "organization".</para></summary>
    [JsonProperty("organization")]
    public string? Organization { get; set; }
    /// <summary><para>Gets or sets the group.</para>
    /// <para>Serializes as "group".</para></summary>
    [JsonProperty("group")]
    public string? Group { get; set; }
    /// <summary><para>Gets sets whether the model is blocked.</para>
    /// <para>Serializes as "is_blocking".</para></summary>
    [JsonProperty("is_blocking")]
    public bool IsBlocking { get; set; }
}
