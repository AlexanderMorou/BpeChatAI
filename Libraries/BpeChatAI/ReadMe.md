# BpeChatAI
[![NuGet](https://img.shields.io/nuget/v/BpeChatAI.svg)](https://www.nuget.org/packages/BpeChatAI)
[![Last Commit](https://img.shields.io/github/last-commit/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/commits/main)
[![GitHub Issues](https://img.shields.io/github/issues/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/issues)
[![Used by](https://img.shields.io/nuget/dt/BpeChatAI.svg)](https://www.nuget.org/packages/BpeChatAI)
[![Contributors](https://img.shields.io/github/contributors/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/graphs/contributors)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

BpeChatAI implements the POCO objects needed to call OpenAI's GPT API.
With its chat Manager, it tracks token counts, handling the API calls for you, and cost-tracking.

Features both streaming and non-streaming API calls to OpenAI's GPT Chat Completion API.

BpeChatAI is uses [BpeTokenizer](https://www.nuget.org/packages/BpeTokenizer/), an adaptation of OpenAI's [tiktoken](https://github.com/openai/tiktoken), for token counting.

This library is built for x64 architectures.