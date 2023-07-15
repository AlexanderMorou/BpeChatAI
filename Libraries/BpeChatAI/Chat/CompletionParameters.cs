using BpeTokenizer;

using BpeChatAI.OpenAI;

using Newtonsoft.Json;

namespace BpeChatAI.Chat;

/// <summary>Represents the parameters for a completion</summary>
public class CompletionParameters
{
    private BytePairEncoder? _encoding;

    /// <summary><para>Initializes a new instance of the <see cref="CompletionParameters"/> class
    /// with the specified <paramref name="model"/>, <paramref name="temperature"/>,
    /// <paramref name="maxTokens"/>, and <paramref name="numPrompts"/>.</para></summary>
    /// <param name="model"></param>
    /// <param name="temperature"></param>
    /// <param name="maxTokens"></param>
    /// <param name="numPrompts"></param>
    public CompletionParameters
        ( string model
        , double? temperature = null
        , int? maxTokens = null
        , int? numPrompts = null)
    {
        this.Model       = model;
        this.Temperature = temperature;
        this.MaxTokens   = maxTokens;
        this.NumPrompts  = numPrompts;
    }

    /// <summary><para>Gets the input cost for the completion.</para>
    /// <para>Dependent on every message in the <see cref="Messages"/>.</para></summary>
    /// <returns></returns>
    public async Task<decimal> GetInputCostAsync()
    {
        const string modelWithUniqueTokenCounts = "gpt-3.5-turbo-0301";
        bool isModelWithUniqueTokenCounts = this.Model == modelWithUniqueTokenCounts;
        int tokenCount = this.Messages.Count * (isModelWithUniqueTokenCounts ? 4 : 3);
        _encoding ??= await BytePairEncodingModels.EncodingForModelAsync(Model);
        // ToDo: Keep track of the changes to
        // https://github.com/openai/openai-cookbook/blob/main/examples/How_to_count_tokens_with_tiktoken.ipynb
        // and update this code accordingly
        foreach (var message in this.Messages)
        {
            if (!string.IsNullOrWhiteSpace(message.Name))
                tokenCount += _encoding.Encode(message.Name).Count + (isModelWithUniqueTokenCounts ? -1 : 1);
            if (!string.IsNullOrWhiteSpace(message.Role))
                tokenCount += _encoding.Encode(message.Role).Count;
            if (!string.IsNullOrWhiteSpace(message.Content))
                tokenCount += _encoding.Encode(message.Content).Count;
        }
        return tokenCount * OpenAIApis.GetInputCostPerToken(Model);
    }

    /// <summary><para>Gets or sets whether the completion should be streamed or not.</para>
    /// <para>Serializes as "stream".</para></summary>
    [JsonProperty("stream")]
    public bool Stream { get; set; } = false;

    /// <summary><para>Gets or sets the model used for the completion.</para>
    /// <para>Serializes as "model".</para></summary>
    [JsonProperty("model")]
    public string Model { get; }

    /// <summary><para>Gets or sets the temperature setting for the completion.</para>
    /// <para>Serializes as "temperature".</para></summary>
    [JsonProperty("temperature")]
    public double? Temperature { get; }

    /// <summary><para>Gets or sets the maximum number of tokens for the completion.</para>
    /// <para>Serializes as "max_tokens".</para></summary>
    [JsonProperty("max_tokens")]
    public int? MaxTokens { get; }

    /// <summary><para>Gets or sets the number of prompts for the completion.</para>
    /// <para>Serializes as "n".</para></summary>
    [JsonProperty("n")]
    public int? NumPrompts { get; set; }

    /// <summary><para>Gets or sets the messages for the completion.</para>
    /// <para>Serializes as "messages".</para></summary>
    [JsonProperty("messages")]
    public List<CompletionMessage> Messages { get; init; } = new List<CompletionMessage>();
}
