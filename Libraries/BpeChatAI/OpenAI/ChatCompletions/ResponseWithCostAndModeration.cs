using BpeChatAI.OpenAI.Moderation;
using ModerationResponse = BpeChatAI.OpenAI.Moderation.Response;
using CompletionResponse = BpeChatAI.OpenAI.ChatCompletions.Response;

namespace BpeChatAI.OpenAI.ChatCompletions;

/// <summary>Represents a completion response with cost.</summary>
public class ResponseWithCostAndModeration
    : CompletionResponse
{
    /// <summary>Initializes a new instance of the <see cref="ResponseWithCostAndModeration"/> class.</summary>
    /// <param name="response">The completion response to seed the initial values of the
    /// <see cref="ResponseWithCostAndModeration"/> instance.</param>
    /// <param name="inputCost">The input cost.</param>
    /// <param name="outputCost">The output cost.</param>
    /// <param name="inputModeration">The <see cref="Result"/> for the input to the chat completion API.</param>
    /// <param name="outputModerations">The list of <see cref="Result"/> for the output from the chat completion API.</param>
    public ResponseWithCostAndModeration
        (Response response
        , decimal inputCost
        , decimal outputCost
        , ModerationResponse? inputModeration = null
        , Dictionary<int, ModerationResponse>? outputModerations = null)
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
        InputModeration  = inputModeration;
        OutputModeration = outputModerations;
    }
    /// <summary><para>Gets the input cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal InputCost { get; }
    /// <summary><para>Gets the output cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal OutputCost { get; }
    /// <summary><para>Gets the total cost.</para><para>This is a convenience property and is not serialized.</para></summary>
    public decimal TotalCost => InputCost + OutputCost;

    /// <summary><para>Gets or sets the moderation result from the input to the chat completion API.</para>
    /// <para>May be null if the input was not moderated.</para></summary>
    public ModerationResponse? InputModeration { get; set; }
    /// <summary><para>Gets or sets the moderation result from the output from the chat completion API.</para>
    /// <para>May be null if the output was not moderated or if the moderation was unable to move past the 
    /// input.</para><para>The key is the index of the choice in the <see cref="Response.Choices"/> list.
    /// If a given choice was not able to be moderated, then the key will be absent.</para>
    /// </summary>
    public Dictionary<int, ModerationResponse>? OutputModeration { get; set; }

    /// <summary><para>Gets whether the input or output, if moderated, was
    /// flagged.</para><para>
    /// Returns <see langword="true"/> if the input was flagged or if any
    /// of the output choices were flagged.</para>
    /// <para>Convenience property for checking if the input or any of the
    /// outputs were flagged and is not serialized.</para></summary>
    public bool IsModeratedAndFlagged
        => (InputModeration?.IsFlagged ?? false)
        || (OutputModeration?.Any(x => x.Value.IsFlagged) ?? false);

}
