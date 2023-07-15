# BpeTokenizer
[![NuGet](https://img.shields.io/nuget/v/BpeTokenizer.svg)](https://www.nuget.org/packages/BpeTokenizer)
[![Last Commit](https://img.shields.io/github/last-commit/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/commits/main)
[![GitHub Issues](https://img.shields.io/github/issues/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/issues)
[![Used by](https://img.shields.io/nuget/dt/BpeTokenizer.svg)](https://www.nuget.org/packages/BpeTokenizer)
[![Contributors](https://img.shields.io/github/contributors/AlexanderMorou/BpeChatAI.svg)](https://github.com/AlexanderMorou/BpeChatAI/graphs/contributors)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

BpeTokenizer is a C# implementation of [tiktoken](https://github.com/openai/tiktoken) written by OpenAI. It is a byte pair encoding tokenizer that can be used to tokenize text into subword units.

As a BpeTokenizer derived from tiktoken, it can be used as a token counter. Useful to ensure that when streaming tokens from the OpenAI API for GPT Chat Completions, you could keep track of the cost related to the software calling the API.

This library is built for x64 architectures.