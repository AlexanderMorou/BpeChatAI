using Newtonsoft.Json;

namespace BpeChatAI.OpenAI.Models;
/// <summary>Expanded Model Info List class used to represent the format
/// the data is actually received in.</summary>
internal class JsonFriendlyDetailsList
{
    [JsonProperty("data")]
    public List<Details> Data { get; set; } = new List<Details>();
}