using BpeChatAI.Models;
using Newtonsoft.Json;

namespace BpeChatAI.Models;
/// <summary>Expanded Model Info List class used to represent the format
/// the data is actually received in.</summary>
internal class OpenAIModelInfoList
{
    [JsonProperty("data")]
    public List<OpenAIModelInfo> Data { get; set; } = new List<OpenAIModelInfo>();
}