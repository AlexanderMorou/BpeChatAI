using Newtonsoft.Json;

using System.Diagnostics;

namespace BpeChatAI.OpenAI.Models;

/// <summary>Represents a model from the OpenAI Models API.</summary>
[DebuggerDisplay($"{{{nameof(Id)}}}")]
public class Details
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
    public List<Permissions>? Permission { get; set; }
    /// <summary>Gets or sets the success status of the request.</summary>
    [JsonIgnore]
    public bool IsSuccess { get; set; }
    /// <summary>
    /// Gets or sets the <see cref="string"/> value denoting the error message
    /// when <see cref="IsSuccess"/> is <see langword="false"/>.</summary>
    [JsonIgnore]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }

}
