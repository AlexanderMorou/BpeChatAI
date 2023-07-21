using BpeChatAI.OpenAI.Moderation;
using ModerationResponse = BpeChatAI.OpenAI.Moderation.Response;
using ChatCompletionResponse = BpeChatAI.OpenAI.ChatCompletions.Response;

namespace BpeChatAI.OpenAI.ChatCompletions;

/// <summary>Represents a completion response with cost.</summary>
public class ResponseWithCostAndModeration
    : ChatCompletionResponse
{
    /// <summary>Initializes a new instance of the <see cref="ResponseWithCostAndModeration"/> class.</summary>
    /// <param name="response">The completion response to seed the initial values of the
    /// <see cref="ResponseWithCostAndModeration"/> instance.</param>
    /// <param name="inputCost">The input cost.</param>
    /// <param name="outputCost">The output cost.</param>
    /// 
    public ResponseWithCostAndModeration
        ( ChatCompletionResponse response
        , decimal inputCost
        , decimal outputCost)
    {
        OutputCost       = outputCost;
        Id               = response.Id;
        Object           = response.Object;
        Created          = response.Created;
        Model            = response.Model;
        Choices          = response.Choices;
        Usage            = response.Usage;
        IsSuccess        = response.IsSuccess;
        ErrorMessage     = response.ErrorMessage;
        Exception        = response.Exception;
        InputCost        = inputCost;
    }
    /// <summary><para>Gets the input cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal InputCost { get; }
    /// <summary><para>Gets the output cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal OutputCost { get; }
    /// <summary><para>Gets the total cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal TotalCost => InputCost + OutputCost;

    /// <summary><para>Gets whether the input or output, if moderated, was
    /// flagged.</para><para>
    /// Returns <see langword="true"/> if the input was flagged or if any
    /// of the output choices were flagged.</para>
    /// <para>Convenience property for checking if the input or any of the
    /// outputs were flagged and is not serialized.</para></summary>
    public bool IsModeratedAndFlagged
        => this.Choices?.Any(x => x.Message?.ModerationResponse?.IsFlagged
                               ?? false)
           ?? false;

}
