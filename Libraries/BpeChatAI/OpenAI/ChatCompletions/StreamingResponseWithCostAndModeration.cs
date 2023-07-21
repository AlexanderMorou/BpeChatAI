using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModerationResponse = BpeChatAI.OpenAI.Moderation.Response;

namespace BpeChatAI.OpenAI.ChatCompletions;
/// <summary>Represents a streaming completion response.</summary>
public class StreamingResponseWithCostAndModeration
    : ApiResponseWithSuccessInformation
{
    /// <summary>Gets or sets the completion once it has been aggregated together.</summary>
    public string? Completion { get; set; }
    /// <summary>Gets or sets the cost of the completion.</summary>
    public decimal OutputCost { get; set; }
    /// <summary>Gets or sets the moderation result.</summary>
    public ModerationResponse? OutputModeration { get; set; }
    /// <summary><para>Gets or sets the index of the completion.</para></summary>
    public int Index { get; set; }

    /// <summary>Gets or sets whether the response was successful or not.</summary>
    [JsonIgnore()]
    [MemberNotNullWhen(true, nameof(Completion))]
    public override bool IsSuccess { get => base.IsSuccess; set => base.IsSuccess=value; }
}
