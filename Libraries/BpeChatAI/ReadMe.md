# BpeChatAI
[![NuGet](https://img.shields.io/nuget/v/BpeChatAI.svg)](https://www.nuget.org/packages/BpeChatAI)
[![Last Commit](https://img.shields.io/github/last-commit/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/commits/master)
[![GitHub Issues](https://img.shields.io/github/issues/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/issues)
[![Used by](https://img.shields.io/nuget/dt/BpeChatAI.svg)](https://www.nuget.org/packages/BpeChatAI)
[![Contributors](https://img.shields.io/github/contributors/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/graphs/contributors)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

BpeChatAI implements the POCO objects needed to call OpenAI's GPT API.
With its chat Manager, it tracks token counts, handling the API calls for you, and cost-tracking.

Features both streaming and non-streaming API calls to OpenAI's GPT Chat Completion API.

BpeChatAI is uses [BpeTokenizer](https://www.nuget.org/packages/BpeTokenizer/), an adaptation of OpenAI's [tiktoken](https://github.com/openai/tiktoken), for token counting.

This library is built for x64 architectures.

## Installation

The BpeChatAI library can be installed via NuGet:

```bash
Install-Package BpeChatAI
```

## Usage

The main class for interaction with the OpenAI Chat Completion API is the `ChatManager`. It supports input and output moderation with cost tracking.

### Instantiation

You can create an instance of the `ChatManager` as follows:

```CSharp
var chatManager = new ChatManager(apiClient, options);
```

Where:
- `apiClient` is an instance of the `ApiClient` class.
- `options` is an instance of the `ChatManagerOptions` class.
 
ChatManagerOptions:
 - `KnownChatCompletionModel` - The known chat completion model to use. Available values:
     - `GPT3PointFiveTurbo` - gpt-3.5-turbo
     - `GPT3PointFiveTurbo_16k` - gpt-3.5-turbo-16k
     - `GPT4` - gpt-4
     - `GPT4_32k` - gpt-4-32k
 - `Temperature` - The temperature to use for the chat completion. 
The default is `null`, which uses the default temperature for the model.
 - `MaxTokens` - The maximum number of tokens to generate in all completion results. Default is `null`, which will generate the maximum number of tokens for the model.
 - `NumPrompts` - The number of prompts to request the API generate. Default is `null`, which will use the default number of prompts for the model (1).
 - `IsModerated` - Whether to use input and output moderation. Default is `true`.
### Interacting with the ChatManager

The `ChatManager` class provides several methods for interacting with the GPT-3 API:
1. `PostAsync`
1. `PostUserMessageAsync`
1. `PostStreamingAsync`
1. `PostStreamingUserMessageAsync`

### `PostAsync`
Posts the parameters to the OpenAI Chat Completion API and returns the response.

```CSharp
var response = await chatManager.PostAsync(cancellationToken);
```

### `PostUserMessageAsync`
Posts a user role message to the parameters, then executes `PostAsync`.

```CSharp
var response = await chatManager.PostUserMessageAsync(message, cancellationToken);
```

### `PostStreamingAsync`
Posts the parameters to the OpenAI Chat Completion API as a streaming request and returns the response as an `IAsyncEnumerable`.

```CSharp
await foreach (var response in chatManager.PostStreamingAsync(cancellationToken))
{
    // Process response
}
```

### `PostStreamingUserMessageAsync`
Posts a user role message to the parameters, then executes `PostStreamingAsync`.

```CSharp
await foreach (var response in chatManager.PostStreamingUserMessageAsync(message, cancellationToken))
{
    // Process response
}
```

### Streaming Details
The `ChatManager` class has an event `StreamTokenReceived` which occurs
when a token is received from the API when a streaming request is made. The
method is intended to be used to print the results of the most recently
received token to the console or to some async service waiting to receive
tokens itself.

Both streaming methods yield a `StreamingResponseWithCostAndModeration` object, where:
 - `Completion` - The `string` completion response from the API.
 - `OutputCost` - The `decimal` cost of the output tokens.
 - `OutputModeration` - The `Moderation.Response` of the output aggregate.
 - `Index` - The `int` index of the generated prompt. Only really relevant when `ChatManager.NumPrompts` > 1.

When `ChatManager.NumPrompts` is 1, the Completion will be added
as an `assistant` `Message` to the `ChatManager`'s Parameters.
The rationale here is this is a `ChatManager` which is intended
to be used in a back-and-forth conversation with the API. 
If `ChatManager.IsModerated` is `true`, the `ChatManager` will also
carry forward the output moderation details for the added `Message`
to avoid a double moderation cost (in time, since the moderation API
is free.)

## Setting Up the OpenAI API Client

You can configure the OpenAI API client using an `appsettings.json` file. Here is an example configuration:

```json
{
  "OpenAI": {
    "ApiKey": "YOUR_API_KEY"
  }
}
```

Replace `"YOUR_API_KEY"` with your actual OpenAI API key.

In your code, you can set up the API client as follows:

```CSharp
var openAIClientSettings =
    new ConfigurationBuilder()
    .SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!)
    .AddJsonFile("appsettings.json")
    .Build()
    .GetSection(ApiClientSettings.SectionName).Get<ApiClientSettings>();
if (openAIClientSettings is null)
{
    Console.WriteLine("OpenAI configuration is missing.");
    return;
}

var apiClient = new ApiClient(openAIClientSettings);
var chatManagerOptions = 
    new ChatManagerOptions
    ( model      : KnownChatCompletionModel.GPT4
    , temperature: 0.9f
    , maxTokens  : 150
    , numPrompts : 1
    , isModerated: true);
```

The `ApiClient` object can then be passed to the `ChatManager` when creating an instance of it.
You may also want to use environment variables to specify your API Key. In that case, you can use the following configuration.
Let's take an example where the OpenAI API Key is stored in the Environment variable `OPENAI_API_KEY`:
```CSharp
var openAIClientSettings = 
    new ApiClientSettings
    { ApiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") };

var apiClient = new ApiClient(openAIClientSettings);
```
This would make it less likely to accidentally expose the API key in a public repository.

