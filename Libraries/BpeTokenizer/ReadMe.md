# BpeTokenizer
[![NuGet](https://img.shields.io/nuget/v/BpeTokenizer.svg)](https://www.nuget.org/packages/BpeTokenizer)
[![Last Commit](https://img.shields.io/github/last-commit/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/commits/master)
[![GitHub Issues](https://img.shields.io/github/issues/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/issues)
[![Used by](https://img.shields.io/nuget/dt/BpeTokenizer.svg)](https://www.nuget.org/packages/BpeTokenizer)
[![Contributors](https://img.shields.io/github/contributors/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/graphs/contributors)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

BpeTokenizer is a C# implementation of [tiktoken](https://github.com/openai/tiktoken) written by OpenAI. It is a byte pair encoding tokenizer that can be used to tokenize text into subword units.

This library is built for x64 architectures.

As a BpeTokenizer derived from tiktoken, it can be used as a token counter. Useful to ensure that when streaming tokens from the OpenAI API for GPT Chat Completions, you could keep track of the cost related to the software calling the API.

To Install BpeTokenizer, run the following command in the Package Manager Console
```
Install-Package BpeTokenizer
```

If you'd prefer to use the .NET CLI, run this command instead:
```
dotnet add package BpeTokenizer
```

## Usage

To use BpeTokenizer, import the namespace:
```CSharp
using BpeTokenizer;
```

Then create an encoder by its model or encoding name:
```CSharp
// By its encoding name:
var encoder = await BytePairEncodingRegistry.GetEncodingAsync("cl100k_base");

// By its model:
var encoder = await BytePairEncodingModels.EncodingForModelAsync("gpt-4");
```
Both variants are async so you can await them, since they will either
access a remote server to download the model or load it from the local cache.

Once you have an encoding, you can encode your text:
```CSharp
var tokens = encoder.Encode("Hello BPE world!"); //Results in: [9906, 426, 1777, 1917, 0]
```
To decode a stream of tokens, you can use the following:
```CSharp
var text = encoder.Decode(tokens); //Results in: "Hello BPE world!"
```

## Supported Encodings/Models:
BpeTokenizer supports the following encodings:
1. cl100k_base
1. p50k_edit
1. p50k_base
1. r50k_base
1. gpt2

You can use these encoding names when creating an encoder:
```CSharp
var cl100kBaseEncoder = await BytePairEncodingRegistry.GetEncodingAsync("cl100k_base");
var p50kEditEncoder   = await BytePairEncodingRegistry.GetEncodingAsync("p50k_edit");
var p50kBaseEncoder   = await BytePairEncodingRegistry.GetEncodingAsync("p50k_base");
var r50kBaseEncoder   = await BytePairEncodingRegistry.GetEncodingAsync("r50k_base");
var gpt2Encoder       = await BytePairEncodingRegistry.GetEncodingAsync("gpt2");
```

The following models are supported (from tiktoken source, embedding in parentheses):
1. Chat (all cl100k_base)
    1. gpt-4         - e.g., gpt-4-0314, etc., plus gpt-4-32k
    1. gpt-3.5-turbo - e.g, gpt-3.5-turbo-0301, -0401, etc.
    1. gpt-35-turbo  - Azure deployment name
1. Text ([future use](https://openai.com/blog/gpt-4-api-general-availability#deprecation-of-older-models-in-the-completions-api), all [cl100k_base](https://github.com/openai/tiktoken/issues/166#issuecomment-1637211143) API availability on Jan 4, 2024)
    1. ada-002
    1. babbage-002
    1. curie-002
    1. davinci-002
    1. gpt-3.5-turbo-instruct
1. Code (all p50k_base)
    1. code-davinci-002
    1. code-davinci-001
    1. code-cushman-002
    1. code-cushman-001
    1. davinci-codex
    1. cushman-codex
1. Edit (all p50k_edit)
    1. text-davinci-edit-001
    1. code-davinci-edit-001
1. Embeddings
    1. text-embedding-ada-002 (cl100k_base)
1. [Legacy](https://openai.com/blog/gpt-4-api-general-availability#deprecation-of-older-models-in-the-completions-api) (no longer available on Jan 4, 2024)
    1. text-davinci-003 (p50k_base)
    1. text-davinci-002 (p50k_base)
    1. text-davinci-001 (r50k_base)
    1. text-curie-001   (r50k_base)
    1. text-babbage-001 (r50k_base)
    1. text-ada-001     (r50k_base)
    1. davinci          (r50k_base)
    1. curie            (r50k_base)
    1. babbage          (r50k_base)
    1. ada              (r50k_base)
1. Old Embeddings (all r50k_base)
    1. text-similarity-davinci-001
    1. text-similarity-curie-001
    1. text-similarity-babbage-001
    1. text-similarity-ada-001
    1. text-search-davinci-doc-001
    1. text-search-curie-doc-001
    1. text-search-babbage-doc-001
    1. text-search-ada-doc-001
    1. code-search-babbage-code-001
    1. code-search-ada-code-001
1. Open Source
    1. gpt2 (gpt2)

You can use these model names when creating an encoder (list not exhaustive):
```CSharp
var gpt4Encoder                     = await BytePairEncodingModels.EncodingForModelAsync("gpt-4");
var textDavinci003Encoder           = await BytePairEncodingModels.EncodingForModelAsync("text-davinci-003");
var textDavinci001Encoder           = await BytePairEncodingModels.EncodingForModelAsync("text-davinci-001");
var codeDavinci002Encoder           = await BytePairEncodingModels.EncodingForModelAsync("code-davinci-002");
var textDavinciEdit001Encoder       = await BytePairEncodingModels.EncodingForModelAsync("text-davinci-edit-001");
var textEmbeddingAda002Encoder      = await BytePairEncodingModels.EncodingForModelAsync("text-embedding-ada-002");
var textSimilarityDavinci001Encoder = await BytePairEncodingModels.EncodingForModelAsync("text-similarity-davinci-001");
var gpt2Encoder                     = await BytePairEncodingModels.EncodingForModelAsync("gpt2");
```

Several of the older models are being deprecated at the start of 2024:
* [Model deprecation information](https://openai.com/blog/gpt-4-api-general-availability#deprecation-of-older-models-in-the-completions-api)

## Token Counting
To count tokens in a given string, you can use the following:
```CSharp
var tokenCount = encoder.CountTokens("Hello BPE world!"); //Results in: 5
```