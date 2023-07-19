using BpeTokenizer;

using BpeChatAI.OpenAI;

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BpeChatAI.OpenAI.Moderation;
using BpeChatAI.OpenAI.ChatCompletions;
using CompletionResponse = BpeChatAI.OpenAI.ChatCompletions.Response;
using ModerationResponse = BpeChatAI.OpenAI.Moderation.Response;
using ModerationParameters = BpeChatAI.OpenAI.Moderation.Parameters;
using CompletionParameters = BpeChatAI.OpenAI.ChatCompletions.Parameters;
// Disabled this so we can have interior comments with <see cref=""/> tags.
#pragma warning disable 1587 // XML comment is not placed on a valid language element

namespace BpeChatAI.Chat;
/// <summary>A class to manage a chat session with OpenAI's chat completion API.</summary>
public class ChatManager
{
    /// <summary>The class that handles the actual API calls.</summary>
    public ApiClient OpenAIApis { get; private set; }

    /// <summary>The parameters to use for the next API call.</summary>
    public CompletionParameters Parameters { get; private set; }

    /// <summary><para>Gets or sets whether the chat session is moderated.</para>
    /// <para>It's recommended that you set this to <see langword="true"/> for
    /// unstructured chat sessions.</para>
    /// <para>Examples of software that may be okay without moderation:
    /// Personal projects that are not publicly accessible, or chat sessions
    /// that are limited to a small group of people who are aware of the
    /// need to self-filter their commentary.</para>
    /// <para>The decision to use or omit moderation is the responsibility
    /// of the developer using this package.</para></summary>
    public bool IsModerated { get; set; } = true;

    /// <summary><para>The moderation results tracked from the <see cref="ApiClient.ModerateAsync(ModerationParameters)"/>.</para></summary>
    public List<Result> LastModerationResult { get; private set; } = new List<Result>();

    /// <summary>Occurs when a streaming token is received.</summary>
    public event EventHandler<StreamTokenReceivedEventArgs>? StreamTokenReceived;

    /// <summary>Creates a new <see cref="ChatManager"/> instance with
    /// the <paramref name="apis"/> and <paramref name="options"/> provided.</summary>
    /// <param name="apis">The <see cref="OpenAIApis"/> instance to use for API calls.</param>
    /// <param name="options">The options to use for the chat session. If
    /// <see langword="null"/>, the default options
    /// will be used in calls to the API.</param>
    public ChatManager(ApiClient apis, ChatManagerOptions options)
        : this(apis, options.Model, options.Temperature, options.MaxTokens, options.NumPrompts) { }

    /// <summary>Creates a new <see cref="ChatManager"/> instance with the
    /// <paramref name="apis"/>, <paramref name="model"/>,
    /// <paramref name="temperature"/>, <paramref name="maxTokens"/>, and
    /// <paramref name="numPrompts"/> provided.</summary>
    /// <param name="apis">The <see cref="OpenAIApis"/> instance to use for API calls.</param>
    /// <param name="model">
    /// <para>The <see cref="KnownChatCompletionModel"/> that establishes the model used by the
    /// <see cref="BpeTokenizer.Encoder"/> to handle tokenization.</para>
    /// <para>Tokenization is key to tracking cost as the OpenAI API uses the same
    /// tokenization process.</para></param>
    /// <param name="temperature"><para>The <see cref="CompletionParameters.Temperature"/>
    /// to use for the chat session.</para><para>Can be changed later by 
    /// calling <see cref="ChangeTemperature(double)"/>.</para></param>
    /// <param name="maxTokens">
    /// <para>The <see cref="CompletionParameters.MaxTokens"/> to use for the chat session.</para>
    /// <para>Can be changed later by calling <see cref="ChangeMaxTokens(int)"/>.</para>
    /// </param>
    /// <param name="numPrompts"><para>The <see cref="CompletionParameters.NumPrompts"/> to use for the chat session.</para>
    /// <para>Can be changed later by calling <see cref="ChangeNumPrompts(int)"/>.</para></param>
    public ChatManager(ApiClient apis, KnownChatCompletionModel model, double? temperature = null, int? maxTokens = null, int? numPrompts = null)
    {
        this.OpenAIApis = apis;
        this.Parameters = new CompletionParameters(model.GetModelName(), temperature, maxTokens, numPrompts);
    }

    /// <summary><para>Posts a user role message to the <see cref="Parameters"/>,
    /// then executes <see cref="PostAsync(CancellationToken)"/>.</para></summary>
    /// <param name="message"><para>The message to post to the <see cref="Parameters"/> under the user role.</para></param>
    /// <param name="cancellationToken"><para>The <see cref="CancellationToken"/> to use for the API call.</para></param>
    /// <returns><para>A <see cref="ResponseWithCostAndModeration"/> instance that represents the response from the API.</para></returns>
    public async Task<ResponseWithCostAndModeration> PostUserMessageAsync(string message, CancellationToken cancellationToken = default)
    {
        this.Parameters.Messages.Add(new Message { Content = message, Role = "user" });
        return await this.PostAsync(cancellationToken);
    }

    /// <summary><para>Posts the <see cref="Parameters"/> to the OpenAI Chat
    /// Completion API and returns the response.</para></summary>
    /// <param name="cancellationToken"><para>The <see cref="CancellationToken"/> to use for the API call.</para></param>
    /// <returns><para>A <see cref="ResponseWithCostAndModeration"/> instance that represents the response from the API.</para></returns>
    public async Task<ResponseWithCostAndModeration> PostAsync(CancellationToken cancellationToken = default)
    {
        ModerationResponse? inputModeration = null;
        if (this.IsModerated)
        {
            var finalMessage = this.Parameters.Messages.LastOrDefault();
            if (!(finalMessage?.Moderated ?? true)
                && !string.IsNullOrWhiteSpace(finalMessage!.Content))
                try
                {
                    var moderationParameters = new ModerationParameters(finalMessage!.Content);
                    inputModeration = await this.OpenAIApis.ModerateAsync(moderationParameters, cancellationToken);
                }
                catch (Exception moderationException)
                {
                    /// If moderation fails and it's a moderated <see cref="ChatManager"/> instance,
                    /// we should fail the API call.
                    return new ResponseWithCostAndModeration
                           ( CompletionResponse.EmptyResponse
                           , inputCost: 0
                           , outputCost: 0)
                           { IsSuccess = false
                           , Exception = moderationException };
                }
        }

        var response = await this.OpenAIApis.CallChatAsync(this.Parameters, inputModeration, cancellationToken);
        if (response.IsSuccess
            && response.Choices != null)
        {
            if ((this.Parameters.NumPrompts ?? 1) == 1)
            {
                var firstChoice = response.Choices.First();
                if (firstChoice?.Message != null)
                    this.Parameters.Messages.Add(firstChoice.Message!);
            }
            this.InputCost += response.InputCost;
            this.OutputCost += response.OutputCost;
        }
        else
        {
            // In some cases InputCost is returned even if the API call fails.
            // This is because we do not always know whether the endpoint 
            // has fully processed a request and started sending back data.
            this.InputCost += response.InputCost;
        }
        return response;
    }

    /// <summary>
    /// <para>Posts the <see cref="Parameters"/> to the OpenAI Chat Completion
    /// API as a streaming request and returns the response as an <see cref="IAsyncEnumerable{T}"/>.</para>
    /// <para>If <see cref="CompletionParameters.NumPrompts"/> is 1, the first
    /// response will be added to <see cref="CompletionParameters.Messages"/>.</para>
    /// <para>Otherwise, no response will be added to <see cref="CompletionParameters.Messages"/>.
    /// </para><para>To receive token-wise updates, register a handler to the
    /// <see cref="StreamTokenReceived"/> event.</para></summary>
    /// <param name="cancellationToken"><para>The <see cref="CancellationToken"/> to use for the API call.</para></param>
    /// <returns><para>An <see cref="IAsyncEnumerable{T}"/> that represents the response from the API.</para>
    /// <para>Each item in the enumerable represents a prompt response.</para>
    /// <para>The number of items returned from this method relates to the
    /// <see cref="CompletionParameters.NumPrompts"/> specified in <see cref="Parameters"/>.</para></returns>
    public async IAsyncEnumerable<string> PostStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = OpenAIApis.CallChatStreamingAsync(this.Parameters, cancellationToken);
        var builders = new Dictionary<int, StringBuilder>();
        bool wasNoCost = false;
        await foreach (var output in response)
        {
            if (output == null
                || !output.IsSuccess
                || cancellationToken.IsCancellationRequested
                || output.Choices == null)
            {
                if (output != null
                    && output.ErrorOnRequest)
                    wasNoCost = true;
                break;
            }
            foreach (var choice in output.Choices)
            {
                if (choice == null)
                    continue;
                if (!builders.TryGetValue(choice.Index, out var builder))
                {
                    builder = new StringBuilder();
                    builders.Add(choice.Index, builder);
                }
                if (choice.Delta != null)
                {
                    builder.Append(choice.Delta.Content);
                    var parameters = new StreamTokenReceivedEventArgs(choice.Delta.Content, choice.Index);
                    this.OnStreamTokenReceived(parameters);
                    this.OutputCost += await parameters.GetOutputCostAsync(this);
                }
            }
        }
        if (!wasNoCost)
            this.InputCost += await Parameters.GetInputCostAsync();
        if ((this.Parameters.NumPrompts ?? 1) == 1
            && builders.Count == 1)
        {
            var firstChoice = builders.First().Value.ToString();
            this.Parameters.Messages.Add
                ( new Message
                  { Role = "assistant"
                  , Content = firstChoice });
        }
        foreach (var stringBuilder in builders)
            yield return stringBuilder.Value.ToString();
    }

    /// <summary><para>Posts a user role message to the <see cref="Parameters"/>,
    /// then executes <see cref="PostStreamingAsync(CancellationToken)"/>.</para>
    /// <para>To receive token-wise updates, register a handler to the
    /// <see cref="StreamTokenReceived"/> event.</para></summary>
    /// <param name="message"><para>The message to post to the <see cref="Parameters"/>
    /// under the user role.</para></param>
    /// <param name="cancellationToken"><para>The <see cref="CancellationToken"/>
    /// to use for the API call.</para></param>
    /// <returns><para>An <see cref="IAsyncEnumerable{T}"/> that represents the response from the API.</para>
    /// <para>Each item in the enumerable represents a prompt response.</para>
    /// <para>The number of items returned from this method relates to the
    /// <see cref="CompletionParameters.NumPrompts"/> specified in <see cref="Parameters"/>.</para></returns>
    public async IAsyncEnumerable<string> PostStreamingUserMessageAsync(string message, [EnumeratorCancellation]CancellationToken cancellationToken = default)
    {
        this.Parameters.Messages.Add(new Message { Content = message, Role = "user" });
        // Use the async enumerable to ensure the EnumeratorCancellation attribute
        // avoids giving a compiler warning.
        await foreach (var response in PostStreamingAsync(cancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
                break;
            yield return response;
        }
    }

    /// <summary><para>Occurs when a token is received from the API when
    /// a streaming request is made.</para></summary>
    /// <param name="e">
    /// <para>The <see cref="StreamTokenReceivedEventArgs"/> that contains the token
    /// and the index of the prompt response it belongs to.</para>
    /// <para>The <see cref="StreamTokenReceivedEventArgs.GetOutputCostAsync(ChatManager)"/>
    /// method can be used to get the cost of the token.</para>
    /// </param>
    protected virtual void OnStreamTokenReceived(StreamTokenReceivedEventArgs e)
        => this.StreamTokenReceived?.Invoke(this, e);

    /// <summary><para>Occurs when a token is received from the API when
    /// a streaming request is made.</para></summary>
    /// <param name="token"><para>The token that was received.</para></param>
    /// <param name="index"><para>The index of the prompt response the token belongs to.</para></param>
    /// <remarks><para>This method is calls the <see cref="OnStreamTokenReceived(StreamTokenReceivedEventArgs)"/>.</para>
    /// <para>Intended to be used by derived classes to raise the <see cref="StreamTokenReceived"/> event.</para></remarks>
    protected void OnStreamTokenReceived(string token, int index)
        => this.OnStreamTokenReceived(new StreamTokenReceivedEventArgs(token, index));

    /// <summary><para>Changes the temperature of the <see cref="Parameters"/>,
    /// but retains the other traits of the <see cref="Parameters"/>.</para></summary>
    /// <param name="temperature"><para>The new temperature to use.</para></param>
    public void ChangeTemperature(double temperature)
    {
        var oldParameters   = this.Parameters;
        this.Parameters     = new CompletionParameters(oldParameters.Model, temperature, oldParameters.MaxTokens, oldParameters.NumPrompts);
        this.Parameters.Messages.AddRange(oldParameters.Messages);
    }
    
    /// <summary><para>Changes the maximum number of tokens of the <see cref="Parameters"/>,
    /// but retains the other traits of the <see cref="Parameters"/>.</para>
    /// <para><see cref="CompletionParameters.MaxTokens"/> identify the maximum number of tokens
    /// the API should generate for the output.</para></summary>
    /// <param name="maxTokens"><para>The maximum number of tokens
    /// the API should generate for the output.</para></param>
    public void ChangeMaxTokens(int maxTokens)
    {
        var oldParameters   = this.Parameters;
        this.Parameters     = new CompletionParameters(oldParameters.Model, oldParameters.Temperature, maxTokens, oldParameters.NumPrompts);
        this.Parameters.Messages.AddRange(oldParameters.Messages);
    }

    /// <summary><para>Changes the number of prompts of the <see cref="Parameters"/>,
    /// but retains the other traits of the <see cref="Parameters"/>.</para>
    /// <para><see cref="CompletionParameters.NumPrompts"/> identify the number
    /// of prompts the API should generate for the output for the same input.</para></summary>
    /// <param name="numPrompts"><para>The maximum number of prompts
    /// the API should generate in the output.</para></param>
    public void ChangeNumPrompts(int numPrompts)
    {
        var oldParameters   = this.Parameters;
        this.Parameters     = new CompletionParameters(oldParameters.Model, oldParameters.Temperature, oldParameters.MaxTokens, numPrompts);
        this.Parameters.Messages.AddRange(oldParameters.Messages);
    }

    /// <summary><para>Changes the <paramref name="temperature"/>, <paramref name="maxTokens"/>
    /// and <paramref name="numPrompts"/> of the <see cref="Parameters"/>,
    /// but retains the other traits of the <see cref="Parameters"/>.</para></summary>
    /// <param name="temperature"><para>The new temperature to use.</para></param>
    /// <param name="maxTokens"><para>The maximum number of tokens
    /// the API should generate for the output.</para></param>
    /// <param name="numPrompts"><para>The maximum number of prompts
    /// the API should generate in the output.</para></param>
    public void ChangeParameters(double? temperature = null, int? maxTokens = null, int? numPrompts = null)
    {
        var oldParameters   = this.Parameters;
        this.Parameters     = new CompletionParameters(oldParameters.Model, temperature, maxTokens, numPrompts);
        this.Parameters.Messages.AddRange(oldParameters.Messages);
    }

    /// <summary><para>Clears the messages of the <see cref="Parameters"/>.</para></summary>
    public void ClearMessages()
        => this.Parameters.Messages.Clear();

    /// <summary><para>Changes the <paramref name="temperature"/> and clears the messages
    /// of the <see cref="Parameters"/>.</para></summary>
    /// <param name="temperature"><para>The maximum number of tokens
    /// the API should generate for the output.</para></param>
    public void ChangeTemperatureAndClearMessages(double temperature)
    {
        this.ChangeTemperature(temperature);
        this.ClearMessages();
    }

    /// <summary><para>Changes the <paramref name="maxTokens"/> and clears the messages
    /// of the <see cref="Parameters"/>.</para></summary>
    /// <param name="maxTokens"><para>The maximum number of tokens
    /// the API should generate for the output.</para></param>
    public void ChangeMaxTokensAndClearMessages(int maxTokens)
    {
        this.ChangeMaxTokens(maxTokens);
        this.ClearMessages();
    }

    /// <summary><para>Changes the <paramref name="numPrompts"/> and clears the messages
    /// of the <see cref="Parameters"/>.</para></summary>
    /// <param name="numPrompts"><para>The maximum number of prompts
    /// the API should generate in the output.</para></param>
    public void ChangeNumPromptsAndClearMessages(int numPrompts)
    {
        this.ChangeNumPrompts(numPrompts);
        this.ClearMessages();
    }

    /// <summary><para>Changes the <paramref name="temperature"/>, <paramref name="maxTokens"/>,
    /// <paramref name="numPrompts"/> and clears the messages of the <see cref="Parameters"/>.</para></summary>
    /// <param name="temperature"><para>The new temperature to use.</para></param>
    /// <param name="maxTokens"><para>The maximum number of tokens
    /// the API should generate for the output.</para></param>
    /// <param name="numPrompts"><para>The maximum number of prompts
    /// the API should generate in the output.</para></param>
    public void ChangeParametersAndClearMessages(double? temperature = null, int? maxTokens = null, int? numPrompts = null)
    {
        this.ChangeParameters(temperature, maxTokens, numPrompts);
        this.ClearMessages();
    }

    /// <summary>Returns the estimated cost incurred over the lifetime of the
    /// <see cref="ChatManager"/> instance for the input tokens.</summary>
    public decimal InputCost { get; private set; }
    /// <summary>Returns the estimated cost incurred over the lifetime of the
    /// <see cref="ChatManager"/> instance for the output tokens.</summary>
    public decimal OutputCost { get; private set; }
    /// <summary>Returns the estimated cost incurred over the lifetime of the
    /// <see cref="ChatManager"/> instance for the input and output tokens.</summary>
    public decimal TotalCost => this.InputCost + this.OutputCost;
}

/// <summary>Provides data for the <see cref="ChatManager.StreamTokenReceived"/> event.</summary>
public class StreamTokenReceivedEventArgs 
    : EventArgs
{
    internal StreamTokenReceivedEventArgs(string? tokenText, int index)
    {
        this.TokenText = tokenText;
        this.Index = index;
    }

    /// <summary>Returns the text of the token received.</summary>
    public string? TokenText { get; }

    /// <summary>Returns the index denoting which prompt the token received is for.</summary>
    public int Index { get; }

    /// <summary>Returns the estimated cost of the <see cref="TokenText"/>.</summary>"/>
    /// <param name="manager"><para>The <see cref="ChatManager"/> instance
    /// to use for the cost calculation.</para></param>
    /// <returns><para>The estimated cost of the <see cref="TokenText"/>.</para></returns>
    /// <exception cref="ArgumentNullException"><para><paramref name="manager"/> is <see langword="null"/>.</para></exception>
    public async Task<decimal> GetOutputCostAsync(ChatManager manager)
    {
        if (manager == null)
            throw new ArgumentNullException(nameof(manager));
        var encoding = await BpeTokenizer.Models.EncodingForModelAsync(manager.Parameters.Model);
        if (this.TokenText == null)
            return 0;
        return encoding.Encode(this.TokenText).Count * ApiClient.GetOutputCostPerToken(manager.Parameters.Model);
    }
}

/// <summary><para>Denotes known chat completion models.</para></summary>
public enum KnownChatCompletionModel
{
    /// <summary><para>GPT 3.5 turbo.</para></summary>
    [KnownChatCompletionModelName("gpt-3.5-turbo")]
    GPT3PointFiveTurbo,
    /// <summary><para>GPT 3.5 turbo 16k token max.</para></summary>
    [KnownChatCompletionModelName("gpt-3.5-turbo-16k")]
    GPT3PointFiveTurbo_16k,
    /// <summary><para>GPT 4 model.</para></summary>
    [KnownChatCompletionModelName("gpt-4")]
    GPT4,
    /// <summary><para>GPT 4 32k token max.</para></summary>
    [KnownChatCompletionModelName("gpt-4-32k")]
    GPT4_32k,
}

[AttributeUsage(AttributeTargets.Field)]
internal class KnownChatCompletionModelNameAttribute
    : Attribute
{
    public KnownChatCompletionModelNameAttribute(string name)
        => this.Name = name;

    public string Name { get; }
}
/// <summary>Provides extension methods for <see cref="KnownChatCompletionModel"/>.</summary>
public static class KnownChatCompletionModelExtensions
{
    private static readonly Dictionary<KnownChatCompletionModel, string> _knownModels;
    static KnownChatCompletionModelExtensions()
    {
        var enumFields =
            typeof(KnownChatCompletionModel)
            .GetFields
             ( BindingFlags.Public
             | BindingFlags.Static);
        _knownModels =
            enumFields
            .Select(enumField => 
                ( fieldValue: (KnownChatCompletionModel)enumField.GetValue(null)!
                , name: enumField.GetCustomAttribute<KnownChatCompletionModelNameAttribute>()?.Name))
            .Where(x => x.name != null)
            .ToDictionary(x => x.fieldValue, x=> x.name!);
    }

    /// <summary>Returns the name of the model.</summary>
    /// <param name="model"><para>The model to get the name of.</para></param>
    /// <returns><para>The name of the model.</para></returns>
    /// <exception cref="ArgumentOutOfRangeException"><para><paramref name="model"/> is not a known model.</para></exception>
    public static string GetModelName(this KnownChatCompletionModel model)
    {
        if (_knownModels.TryGetValue(model, out var name))
            return name;
        throw new ArgumentOutOfRangeException(nameof(model));
    }
}