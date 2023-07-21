using BpeChatAI.OpenAI;

using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.Models;

/// <summary><para>Represents the models retrieved from <see cref="ApiClient.ListOpenAIModelsAsync"/>
/// flattened into a list without the need for a "Data" property.</para></summary>
public class DetailsList
    : List<Details>
    , IApiResponseWithSuccessInformation
{
    /// <summary>
    /// <para>Gets or sets the success status of the request.</para>
    /// <para>When <see langword="false"/>, <see cref="ErrorMessage"/> will be populated.</para>
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess { get; set; }
    /// <summary>
    /// <para>Gets or sets the <see cref="string"/> value denoting the error message
    /// when <see cref="IsSuccess"/> is <see langword="false"/>.</para>
    /// <para>When <see cref="IsSuccess"/> is <see langword="true"/>, this
    /// will be <see langword="null"/>.</para></summary>
    [JsonIgnore]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }
}
