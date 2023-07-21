using BpeTokenizer;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics.Metrics;
using BpeChatAI.OpenAI.Models;
using BpeChatAI.OpenAI.ChatCompletions;
using BpeChatAI.OpenAI.Moderation;
using ModerationResponse = BpeChatAI.OpenAI.Moderation.Response;
using ChatCompletionResponse = BpeChatAI.OpenAI.ChatCompletions.Response;

using ChatCompletionParameters = BpeChatAI.OpenAI.ChatCompletions.Parameters;
using ModerationParameters = BpeChatAI.OpenAI.Moderation.Parameters;
namespace BpeChatAI.OpenAI;

/// <summary>Represents the OpenAI API endpoints and logic to interface with them.</summary>
public partial class ApiClient
{
    /// <summary>Represents the endpoint URI for chat completions.</summary>
    public const string CompletionsEndpointUri = "https://api.openai.com/v1/chat/completions";
    /// <summary>Represents the endpoint URI for models.</summary>
    public const string ModelsEndpointUri = "https://api.openai.com/v1/models";
    /// <summary>Represents the endpoint URI for obtaining moderation results.</summary>
    public const string ModerationsEndpointUri = "https://api.openai.com/v1/moderations";

    /// <summary>Initializes a new instance of the <see cref="ApiClient"/> class
    /// with the specified <paramref name="settings"/>.</summary>
    /// <param name="settings">The <see cref="ApiClientSettings"/> value containing the
    /// API key needed to access OpenAI API endpoints.</param>
    public ApiClient(ApiClientSettings settings)
        => this.Settings = settings;

    /// <summary>Gets the <see cref="string"/> value denoting the API key needed to access OpenAI API endpoints.</summary>
    public string ApiKey
        => this.Settings.ApiKey!;

    /// <summary>Gets the <see cref="ApiClientSettings"/> value containing the
    /// API key needed to access OpenAI API endpoints.</summary>
    public ApiClientSettings Settings { get; }

    /// <summary>Lists a series of OpenAI models as a list.</summary>
    public async Task<DetailsList> ListOpenAIModelsAsync()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage? response = null;

        try
        {
            response = await httpClient.GetAsync(ModelsEndpointUri);
        }
        catch (Exception e)
        {
            return new DetailsList()
            { IsSuccess = false
            , Exception = e };
        }

        var models = new DetailsList();

        if (response.IsSuccessStatusCode)
        {
            string? resultJson;
            try
            {
                resultJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                models.IsSuccess = false;
                models.Exception = e;
                return models;
            }
            var responseObject = JsonConvert.DeserializeObject<JsonFriendlyDetailsList>(resultJson);

            if (responseObject?.Data != null)
            {
                models.AddRange(responseObject.Data);
                models.Sort((x, y) => x.Id == null ? y.Id == null ? 0 : -1 : x.Id.CompareTo(y.Id));
                models.IsSuccess = true;
            }
            else
            {
                models.IsSuccess = false;
                models.ErrorMessage = $"Error: Could not deserialize into an {nameof(JsonFriendlyDetailsList)}.";
            }
        }
        else
        {
            models.IsSuccess = false;
            models.ErrorMessage = $"Error: {response.StatusCode}";
        }
        return models;
    }

    /// <summary>Obtains the <see cref="Details"/> for the specified
    /// <paramref name="modelId"/>.</summary>
    /// <param name="modelId">The name of the model to retrieve information for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to exit the task response early.</param>
    /// <returns><para>A <see cref="Details"/> instance which contains information
    /// about the <paramref name="modelId"/> specified.</para><para>Also contains
    /// success/error information.</para></returns>
    public async Task<Details> GetOpenAIModelInfoAsync(string modelId, CancellationToken cancellationToken = default)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        HttpResponseMessage? response;
        try
        {
            response = await httpClient.GetAsync($"{ModelsEndpointUri}/{modelId}", cancellationToken);
        }
        catch (Exception e)
        {
            return new Details
                   { Exception = e
                   , IsSuccess = false };
        }

        var modelInfo = new Details();
        if (response.IsSuccessStatusCode)
        {
            string? resultJson;
            try
            {
                resultJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception e)
            {
                modelInfo.Exception = e;
                modelInfo.IsSuccess = false;
                return modelInfo;
            }
            var deserializedModelInfo = JsonConvert.DeserializeObject<Details>(resultJson);
            if (deserializedModelInfo != null)
            {
                modelInfo = deserializedModelInfo;
                modelInfo.IsSuccess = true;
            }
            else
            {
                modelInfo.ErrorMessage = $"Error: {response.StatusCode}";
                modelInfo.IsSuccess = false;
            }
        }
        else
        {
            modelInfo.ErrorMessage = $"Error: {response.StatusCode}";
            modelInfo.IsSuccess = false;
        }

        return modelInfo;
    }

    /// <summary><para>Calls the OpenAI chat completion endpoint with the <paramref name="parameters"/> and
    /// <paramref name="cancellationToken"/> provided.</para><para>Explicitly sets
    /// <see cref="ChatCompletionParameters.Stream"/> to <see langword="false"/>.</para></summary>
    /// <param name="parameters">The <see cref="ChatCompletionParameters"/> which detail the request 
    /// to be made.</param>
    /// <param name="moderateCompletions"><para>Whether or not to moderate the chat completions.</para></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> which can be used to
    /// cancel the request.</param>
    /// <returns><para>A <see cref="ResponseWithCostAndModeration"/> which contains the
    /// <see cref="ChatCompletions.Response"/> and the cost of the request.</para></returns>
    public async Task<ResponseWithCostAndModeration> CallChatAsync
        ( ChatCompletionParameters parameters
        , bool moderateCompletions
        , CancellationToken cancellationToken = default)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        parameters.Stream = false;

        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
        try
        {
            var response = await httpClient.PostAsync(CompletionsEndpointUri, content, cancellationToken);
            var completionResponse = new ChatCompletions.Response();

            if (response.IsSuccessStatusCode)
            {
                string resultJson = await response.Content.ReadAsStringAsync(cancellationToken);
                completionResponse = JsonConvert.DeserializeObject<ChatCompletions.Response>(resultJson);
                if (completionResponse != null)
                    completionResponse.IsSuccess = true;
                else
                    completionResponse =
                        new ChatCompletions.Response
                        { IsSuccess = false
                        , ErrorMessage = "Error: Could not deserialize." };
            }
            else
            {
                completionResponse.IsSuccess = false;
                completionResponse.ErrorMessage = $"Error: {response.StatusCode}";
            }

            decimal outputCost = 0
                  , inputCost = 0;
            if (completionResponse.Usage != null)
            {
                outputCost = GetOutputCostPerToken(completionResponse.Model!) * completionResponse.Usage.CompletionTokens;
                inputCost = GetInputCostPerToken(completionResponse.Model!) * completionResponse.Usage.PromptTokens;
            }
            else
                inputCost = await parameters.GetInputCostAsync();


            // If we have an input moderation, we need to moderate the output
            // choices as well. Users of this library can choose how they respond
            // to the moderation results.
            if (moderateCompletions && completionResponse.IsSuccess)
            {
                var outputModerations = new Dictionary<int, ModerationResponse>();
                if (completionResponse.Choices != null)
                    foreach (ResponseChoice choice in completionResponse.Choices)
                        if (choice.Message?.Content != null)
                        {
                            var moderationInput = new ModerationParameters(choice.Message.Content);
                            var outputModerationResponse = await ModerateAsync(moderationInput);
                            if (outputModerationResponse != null && outputModerationResponse.IsSuccess)
                                outputModerations.Add(choice.Index, outputModerationResponse);
                        }

                return new ResponseWithCostAndModeration(completionResponse, inputCost, outputCost, outputModerations);
            }
            else
                return new ResponseWithCostAndModeration(completionResponse, inputCost, outputCost);
        }
        catch (Exception e)
        {
            var inputCost = await parameters.GetInputCostAsync();
            return new ResponseWithCostAndModeration
                ( new ChatCompletionResponse
                    { IsSuccess = false
                    , Exception = e }
                , inputCost, outputCost: 0);
        }
    }

    /// <summary><para>Calls the OpenAI chat completion endpoint with the <paramref name="parameters"/> and <paramref name="cancellationToken"/> provided.</para>
    /// <para>Explicitly sets <see cref="ChatCompletionParameters.Stream"/> to <see langword="true"/>.</para>
    /// <para>Moderation is not supported for this endpoint implementation due to the streaming nature. It is left
    /// as an exercise to the caller.</para></summary>
    /// <param name="parameters">The <see cref="ChatCompletionParameters"/> which detail the request
    /// to be made.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> which can be used to
    /// cancel the request.</param>
    /// <returns><para>An <see cref="IAsyncEnumerable{T}"/> of <see cref="StreamResponse"/> which
    /// contains individual <see cref="ChatCompletions.Response"/>s as they are received from the
    /// server.</para>
    /// <para>Each response represents roughly a token.
    /// <see cref="StreamResponseChoice.FinishReason"/> for an indicator of why the
    /// <see cref="IAsyncEnumerable{T}"/> is finished.</para></returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="parameters"/> is
    /// <see langword="null"/>.</exception>
    /// 
    public async IAsyncEnumerable<StreamResponse> CallChatStreamingAsync
        ( ChatCompletionParameters parameters
        , [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (parameters == null)
            throw new ArgumentNullException(nameof(parameters));
        parameters.Stream = true;
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, CompletionsEndpointUri) { Content = content };
        httpRequest.Headers.TransferEncodingChunked = true;
        bool yieldExceptionState = false;
        bool errorOnRequest = false;
        Exception? exceptionalState = null;
        HttpResponseMessage? response;
        try
        {
            response = await
                httpClient.SendAsync
                ( httpRequest
                , HttpCompletionOption.ResponseHeadersRead
                , cancellationToken);
        }
        catch (TaskCanceledException)
        {
            errorOnRequest = yieldExceptionState = true;
            goto yieldFail;
        }
        // statement block vs single statement because of goto.
        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                Stream? stream;
                try
                {
                    stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                }
                catch (Exception e)
                {
                    exceptionalState = e;
                    yieldExceptionState = true;
                    goto yieldFail;
                }
                // statement block vs single statement because of goto.
                using (var streamReader = new StreamReader(stream))
                {
                    while (true)
                    {
                        string? currentLine;
                        try
                        {
                            currentLine = await streamReader.ReadLineAsync(cancellationToken);
                        }
                        catch (Exception e)
                        {
                            yieldExceptionState = true;
                            exceptionalState = e;
                            goto yieldFail;
                        }

                        if (currentLine == "data: [DONE]"
                            || currentLine == null)
                            break;
                        if (currentLine == string.Empty)
                            continue;
                        // Tweak the currentLine so it's deserializeable as JSON.
                        currentLine = $"{{{currentLine}}}";
                        // statement blocks vs single statement because of goto.
                        using (var textReader = new StringReader(currentLine))
                        using (var jsonReader = new JsonTextReader(textReader))
                        {
                            var serializer = JsonSerializer.Create();

                            if (jsonReader.Read()
                                && jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var streamingCompletionResponse = serializer.Deserialize<StreamResponseEnvelope>(jsonReader);
                                if (streamingCompletionResponse?.Data is StreamResponse currentResponse)
                                {
                                    currentResponse.IsSuccess = true;
                                    yield return currentResponse;
                                }
                            }
                        }
                    }
                }
            }
            else
                yield return new StreamResponse
                             { IsSuccess = false, ErrorMessage = $"Error: {response.StatusCode}" };
        }
        // A necessary evil, since you cannot yield in a try block.
        yieldFail: 
        if (yieldExceptionState)
            yield return new StreamResponse
            { IsSuccess = false, Exception = exceptionalState, ErrorOnRequest = errorOnRequest };
    }

    /// <summary>Calls the OpenAI moderation endpoint with the <paramref name="input"/>
    /// provided.</summary>
    /// <param name="input"><para>The <see cref="ModerationParameters"/> to send to the OpenAI API.</para></param>
    /// <param name="cancellationToken"><para>An optional <see cref="CancellationToken"/>
    /// used to cancel the request.</para></param>
    /// <returns><para>A moderation response from the OpenAI API, possibly null.</para></returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public async Task<ModerationResponse> ModerateAsync
        ( ModerationParameters input
        , CancellationToken cancellationToken = default)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

        var jsonContent = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(ModerationsEndpointUri, jsonContent, cancellationToken);
        }
        catch (Exception e)
        {
            return new ModerationResponse()
                   { IsSuccess = false
                   , Exception = e };
        }

        if (response.IsSuccessStatusCode)
        {
            string jsonString;

            try
            {
                jsonString = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception e)
            {
                return new ModerationResponse()
                       { IsSuccess = false
                       , Exception = e };
            }
            ModerationResponse? moderationResponse;
            try
            {
                moderationResponse = JsonConvert.DeserializeObject<ModerationResponse>(jsonString);
            }
            catch (Exception e)
            {
                return new ModerationResponse()
                       { IsSuccess = false
                       , Exception = e };
            }
            if (moderationResponse != null)
                moderationResponse.IsSuccess = true;
            else
                moderationResponse =
                    new ModerationResponse()
                    { IsSuccess = false
                    , ErrorMessage = $"Could not deserialize the response received into a {nameof(ModerationResponse)}." };
            return moderationResponse;
        }
        else
            return new ModerationResponse()
                   { IsSuccess = false
                   , ErrorMessage = $"Could not deserialize the response. Status code: {response.StatusCode}, Error: {response.ReasonPhrase}" };
    }

    // ToDo: Allow the model costs to be user-defined. Sometimes the user may have a different cost than
    // the default due to agreements with OpenAI.
    private static readonly Dictionary<string, TokenCosts> ModelCosts
        = new Dictionary<string, TokenCosts>
          { { "gpt-4-32k"        , new (0.00006m  , 0.00012m  ) }
          , { "gpt-4"            , new (0.00003m  , 0.00006m  ) }
          , { "gpt-3.5-turbo-16K", new (0.000003m , 0.000004m ) }
          , { "gpt-3.5-turbo"    , new (0.0000015m, 0.000002m ) }
          , { "text-ada-001"     , new (0.0000004m, 0.0000016m) }
          , { "text-babbage-001" , new (0.0000006m, 0.0000024m) }
          , { "text-curie-001"   , new (0.000003m , 0.000012m ) }
          , { "text-davinci-00"  , new (0.00003m  , 0.00012m  ) } };


    /// <summary>
    /// Returns the cost per token for inputs into the model.
    /// </summary>
    /// <param name="modelId">The name of the model.</param>
    /// <returns>A <see cref="decimal"/> value denoting the cost per input token.</returns>
    public static decimal GetInputCostPerToken(string modelId)
    {
        foreach (var (key, value) in ModelCosts)
            if (modelId.StartsWith(key))
                return value.InputCost;
        return 0;
    }

    /// <summary>Returns the cost per token for outputs from the model.</summary>
    /// <param name="modelId">The name of the model.</param>
    /// <returns>A <see cref="decimal"/> value denoting the cost per output token.</returns>
    public static decimal GetOutputCostPerToken(string modelId)
    {
        foreach (var (key, value) in ModelCosts)
            if (modelId.StartsWith(key))
                return value.OutputCost;
        return 0;
    }
}
