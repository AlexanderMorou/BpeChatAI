﻿using BpeTokenizer;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using BpeChatAI.Moderation;
using BpeChatAI.Chat;
using BpeChatAI.Models;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Diagnostics.Metrics;

namespace BpeChatAI.OpenAI;

/// <summary>Represents the OpenAI API endpoints and logic to interface with them.</summary>
public partial class OpenAIApis
{
    /// <summary>Represents the endpoint URI for chat completions.</summary>
    public const string CompletionsEndpointUri = "https://api.openai.com/v1/chat/completions";
    /// <summary>Represents the endpoint URI for models.</summary>
    public const string ModelsEndpointUri = "https://api.openai.com/v1/models";
    /// <summary>Represents the endpoint URI for obtaining moderation results.</summary>
    public const string ModerationsEndpointUri = "https://api.openai.com/v1/moderations";

    /// <summary>Initializes a new instance of the <see cref="OpenAIApis"/> class
    /// with the specified <paramref name="settings"/>.</summary>
    /// <param name="settings">The <see cref="OpenAISettings"/> value containing the
    /// API key needed to access OpenAI API endpoints.</param>
    public OpenAIApis(OpenAISettings settings)
        => this.Settings = settings;

    /// <summary>Gets the <see cref="string"/> value denoting the API key needed to access OpenAI API endpoints.</summary>
    public string ApiKey
        => this.Settings.ApiKey!;

    /// <summary>Gets the <see cref="OpenAISettings"/> value containing the
    /// API key needed to access OpenAI API endpoints.</summary>
    public OpenAISettings Settings { get; }

    /// <summary>Lists a series of OpenAI models as a list.</summary>
    public async Task<OpenAIModelList> ListOpenAIModelsAsync()
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
            return new OpenAIModelList()
            { IsSuccess = false
            , Exception = e };
        }

        var openAIModels = new OpenAIModelList();

        if (response.IsSuccessStatusCode)
        {
            string? resultJson;
            try
            {
                resultJson = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                openAIModels.IsSuccess = false;
                openAIModels.Exception = e;
                return openAIModels;
            }
            var responseObject = JsonConvert.DeserializeObject<OpenAIModelInfoList>(resultJson);

            if (responseObject?.Data != null)
            {
                openAIModels.AddRange(responseObject.Data);
                openAIModels.Sort((x, y) => x.Id == null ? y.Id == null ? 0 : -1 : x.Id.CompareTo(y.Id));
                openAIModels.IsSuccess = true;
            }
            else
            {
                openAIModels.IsSuccess = false;
                openAIModels.ErrorMessage = $"Error: Could not deserialize into an {nameof(OpenAIModelInfoList)}.";
            }
        }
        else
        {
            openAIModels.IsSuccess = false;
            openAIModels.ErrorMessage = $"Error: {response.StatusCode}";
        }
        return openAIModels;
    }

    /// <summary>Obtains the <see cref="OpenAIModelInfo"/> for the specified
    /// <paramref name="modelId"/>.</summary>
    /// <param name="modelId">The name of the model to retrieve information for.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> used to exit the task response early.</param>
    /// <returns><para>A <see cref="OpenAIModelInfoEx"/> instance which contains information
    /// about the <paramref name="modelId"/> specified.</para><para>Also contains
    /// success/error information.</para></returns>
    public async Task<OpenAIModelInfoEx> GetOpenAIModelInfoAsync(string modelId, CancellationToken cancellationToken = default)
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
            return new OpenAIModelInfoEx
                   { Exception = e
                   , IsSuccess = false };
        }

        var openAIModelInfoEx = new OpenAIModelInfoEx();
        if (response.IsSuccessStatusCode)
        {
            string? resultJson;
            try
            {
                resultJson = await response.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (Exception e)
            {
                openAIModelInfoEx.Exception = e;
                openAIModelInfoEx.IsSuccess = false;
                return openAIModelInfoEx;
            }
            var modelInfo = JsonConvert.DeserializeObject<OpenAIModelInfo>(resultJson);
            if (modelInfo != null)
            {
                openAIModelInfoEx.Id = modelInfo.Id;
                openAIModelInfoEx.Object = modelInfo.Object;
                openAIModelInfoEx.OwnedBy = modelInfo.OwnedBy;
                openAIModelInfoEx.Permission = modelInfo.Permission;
                openAIModelInfoEx.IsSuccess = true;
            }
            else
            {
                openAIModelInfoEx.ErrorMessage = $"Error: {response.StatusCode}";
                openAIModelInfoEx.IsSuccess = false;
            }
        }
        else
        {
            openAIModelInfoEx.ErrorMessage = $"Error: {response.StatusCode}";
            openAIModelInfoEx.IsSuccess = false;
        }

        return openAIModelInfoEx;
    }

    /// <summary><para>Calls the OpenAI chat completion endpoint with the <paramref name="parameters"/> and
    /// <paramref name="cancellationToken"/> provided.</para><para>Explicitly sets
    /// <see cref="CompletionParameters.Stream"/> to <see langword="false"/>.</para></summary>
    /// <param name="parameters">The <see cref="CompletionParameters"/> which detail the request 
    /// to be made.</param>
    /// <param name="inputModeration"><para>The <see cref="ModerationResponse"/> to associate with
    /// this request.</para></param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> which can be used to
    /// cancel the request.</param>
    /// <returns><para>A <see cref="CompletionResponseWithCostAndModeration"/> which contains the
    /// <see cref="CompletionResponse"/> and the cost of the request.</para></returns>
    public async Task<CompletionResponseWithCostAndModeration> CallChatAsync
        ( CompletionParameters parameters
        , ModerationResponse? inputModeration = null
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
            var completionResponse = new CompletionResponse();

            if (response.IsSuccessStatusCode)
            {
                string resultJson = await response.Content.ReadAsStringAsync(cancellationToken);
                completionResponse = JsonConvert.DeserializeObject<CompletionResponse>(resultJson);
                if (completionResponse != null)
                    completionResponse.IsSuccess = true;
                else
                    completionResponse =
                        new CompletionResponse
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
            if (inputModeration != null && completionResponse.IsSuccess)
            {
                var outputModerations = new Dictionary<int, ModerationResponse>();
                if (completionResponse.Choices != null)
                    foreach (CompletionChoice choice in completionResponse.Choices)
                        if (choice.Message?.Content != null)
                        {
                            var moderationInput = new ModerationInput(choice.Message.Content);
                            var outputModerationResponse = await ModerateAsync(moderationInput);
                            if (outputModerationResponse != null && outputModerationResponse.IsSuccess)
                                outputModerations.Add(choice.Index, outputModerationResponse);
                        }

                return new CompletionResponseWithCostAndModeration(completionResponse, inputCost, outputCost, inputModeration, outputModerations);
            }
            else
                return new CompletionResponseWithCostAndModeration(completionResponse, inputCost, outputCost, inputModeration);
        }
        catch (Exception e)
        {
            var inputCost = await parameters.GetInputCostAsync();
            return new CompletionResponseWithCostAndModeration
                ( new CompletionResponse
                    { IsSuccess = false
                    , Exception = e }
                , inputCost, outputCost: 0);
        }
    }

    /// <summary><para>Calls the OpenAI chat completion endpoint with the <paramref name="parameters"/> and <paramref name="cancellationToken"/> provided.</para>
    /// <para>Explicitly sets <see cref="CompletionParameters.Stream"/> to <see langword="true"/>.</para></summary>
    public async IAsyncEnumerable<StreamingCompletionResponse> CallChatStreamingAsync
        ( CompletionParameters parameters
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
                            goto yieldFailInsideUsing;
                        }

                        if (currentLine == "data: [DONE]"
                            || currentLine == null)
                            break;
                        if (currentLine == string.Empty)
                            continue;
                        currentLine = $"{{{currentLine}}}";
                        // statement blocks vs single statement because of goto.
                        using (var textReader = new StringReader(currentLine))
                        using (var jsonReader = new JsonTextReader(textReader))
                        {
                            var serializer = JsonSerializer.Create();

                            if (jsonReader.Read()
                                && jsonReader.TokenType == JsonToken.StartObject)
                            {
                                var streamingCompletionResponse = serializer.Deserialize<StreamingCompletionResponseEnvelope>(jsonReader);
                                if (streamingCompletionResponse?.Data is StreamingCompletionResponse currentResponse)
                                {
                                    currentResponse.IsSuccess = true;
                                    yield return currentResponse;
                                }
                            }
                        }
                    }
                    yieldFailInsideUsing:;
                }
            }
            else
                yield return new StreamingCompletionResponse
                             { IsSuccess = false, ErrorMessage = $"Error: {response.StatusCode}" };
        }
        yieldFail:
        if (yieldExceptionState)
            yield return new StreamingCompletionResponse
            { IsSuccess = false, Exception = exceptionalState, ErrorOnRequest = errorOnRequest };
    }

    /// <summary>Calls the OpenAI moderation endpoint with the <paramref name="input"/>
    /// provided.</summary>
    /// <param name="input"><para>The <see cref="ModerationInput"/> to send to the OpenAI API.</para></param>
    /// <returns><para>A moderation response from the OpenAI API, possibly null.</para></returns>
    /// <exception cref="ArgumentNullException"><paramref name="input"/> is <see langword="null"/>.</exception>
    public async Task<ModerationResponse?> ModerateAsync(ModerationInput input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

        var jsonContent = new StringContent(JsonConvert.SerializeObject(input), Encoding.UTF8, "application/json");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync(ModerationsEndpointUri, jsonContent);
        }
        catch (Exception e)
        {
            return new ModerationResponse()
                   { IsSuccess = false
                   , Exception = e };
        }

        if (response.IsSuccessStatusCode)
        {
            var jsonString = await response.Content.ReadAsStringAsync();
            var moderationResponse = JsonConvert.DeserializeObject<ModerationResponse>(jsonString);
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
    private static readonly Dictionary<string, InputOutputCostsPerToken> ModelCosts
        = new Dictionary<string, InputOutputCostsPerToken>
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
