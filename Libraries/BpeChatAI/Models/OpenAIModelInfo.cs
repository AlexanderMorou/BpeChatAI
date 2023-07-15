using Newtonsoft.Json;

using System.Diagnostics;

namespace BpeChatAI.Models;

/// <summary>Represents a model from the OpenAI Models API.</summary>
[DebuggerDisplay($"{{{nameof(Id)}}}")]
public class OpenAIModelInfo
{
    /// <summary><para>Gets or sets the Id of the model.</para>
    /// <para>Serializes as "id".</para></summary>
    [JsonProperty("id")]
    public string? Id { get; set; }
    /// <summary><para>Gets or sets the type of object represented by the model.</para>
    /// <para>Serializes as "object".</para></summary>
    [JsonProperty("object")]
    public string? Object { get; set; }
    /// <summary><para>Gets or sets the owner of the model.</para>
    /// <para>Serializes as "owner".</para></summary>
    [JsonProperty("owned_by")]
    public string? OwnedBy { get; set; }
    /// <summary><para>Gets or sets set of permissions 
    /// indicating details about the allowances on the model.</para>
    /// <para>Serializes as "permissions".</para></summary>
    [JsonProperty("permission")]
    public List<OpenAIModelPermission>? Permission { get; set; }
}
