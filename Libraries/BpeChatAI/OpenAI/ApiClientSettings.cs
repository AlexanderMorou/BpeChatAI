using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BpeChatAI.OpenAI;
/// <summary>Represents the settings for the OpenAI API endpoints.</summary>
public class ApiClientSettings
{
    /// <summary>Denotes the section name that should be used for clarity.</summary>
    public const string SectionName = "OpenAI";
    /// <summary>Represents the API key needed to access OpenAI API endpoints.</summary>
    [JsonPropertyName("apiKey"), JsonRequired]
    public string? ApiKey { get; set; }
}
