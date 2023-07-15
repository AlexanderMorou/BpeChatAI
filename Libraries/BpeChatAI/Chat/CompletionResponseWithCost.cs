namespace BpeChatAI.Chat;

/// <summary>Represents a completion response with cost.</summary>
public class CompletionResponseWithCost
    : CompletionResponse
{
    /// <summary>Initializes a new instance of the <see cref="CompletionResponseWithCost"/> class.</summary>
    /// <param name="response">The completion response to seed the initial values of the
    /// <see cref="CompletionResponseWithCost"/> instance.</param>
    /// <param name="outputCost">The output cost.</param>
    /// <param name="inputCost">The input cost.</param>
    public CompletionResponseWithCost
        ( CompletionResponse response
        , decimal outputCost
        , decimal inputCost)
    {
        this.OutputCost   = outputCost;
        this.Id           = response.Id;
        this.Object       = response.Object;
        this.Created      = response.Created;
        this.Model        = response.Model;
        this.Choices      = response.Choices;
        this.Usage        = response.Usage;
        this.IsSuccess    = response.IsSuccess;
        this.ErrorMessage = response.ErrorMessage;
        this.Exception    = response.Exception;
        this.InputCost    = inputCost;
    }
    /// <summary>Gets the input cost.</summary>
    public decimal InputCost { get; }
    /// <summary>Gets the output cost.</summary>
    public decimal OutputCost { get; }
    /// <summary>Gets the total cost.</summary>
    public decimal TotalCost => InputCost + OutputCost;
}
