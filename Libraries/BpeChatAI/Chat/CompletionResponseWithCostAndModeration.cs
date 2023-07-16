using BpeChatAI.Moderation;

namespace BpeChatAI.Chat;

/// <summary>Represents a completion response with cost.</summary>
public class CompletionResponseWithCostAndModeration
    : CompletionResponse
{
    /// <summary>Initializes a new instance of the <see cref="CompletionResponseWithCostAndModeration"/> class.</summary>
    /// <param name="response">The completion response to seed the initial values of the
    /// <see cref="CompletionResponseWithCostAndModeration"/> instance.</param>
    /// <param name="inputCost">The input cost.</param>
    /// <param name="outputCost">The output cost.</param>
    /// <param name="inputModeration">The <see cref="ModerationResult"/> for the input to the chat completion API.</param>
    /// <param name="outputModerations">The list of <see cref="ModerationResult"/> for the output from the chat completion API.</param>
    public CompletionResponseWithCostAndModeration
        (CompletionResponse response
        , decimal inputCost
        , decimal outputCost
        , ModerationResponse? inputModeration = null
        , Dictionary<int, ModerationResponse>? outputModerations = null)
    {
        this.OutputCost       = outputCost;
        this.Id               = response.Id;
        this.Object           = response.Object;
        this.Created          = response.Created;
        this.Model            = response.Model;
        this.Choices          = response.Choices;
        this.Usage            = response.Usage;
        this.IsSuccess        = response.IsSuccess;
        this.ErrorMessage     = response.ErrorMessage;
        this.Exception        = response.Exception;
        this.InputCost        = inputCost;
        this.InputModeration  = inputModeration;
        this.OutputModeration = outputModerations;
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
    /// input.</para><para>The key is the index of the choice in the <see cref="CompletionResponse.Choices"/> list.
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
        => (this.InputModeration?.IsFlagged ?? false)
        || (this.OutputModeration?.Any(x => x.Value.IsFlagged) ?? false);

}
