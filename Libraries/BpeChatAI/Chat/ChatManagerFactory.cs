using BpeChatAI.OpenAI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpeChatAI.Chat;
/// <summary><para>Represents a factory for creating <see cref="ChatManager"/> instances if
/// this is used with Dependency Injection.</para></summary>
public class ChatManagerFactory
{
    /// <summary>Initializes a new instance of the <see cref="ChatManagerFactory"/>
    /// class with the specified <paramref name="openAIApis"/>.</summary>
    /// <param name="openAIApis">The <see cref="OpenAIApis"/> instance
    /// to use for creating <see cref="ChatManager"/>.<para>Represents the OpenAI
    /// API endpoints.</para></param>
    public ChatManagerFactory(OpenAIApis openAIApis)
        => this.OpenAIApis = openAIApis;

    /// <summary><para>Gets the <see cref="OpenAIApis"/> instance to use for creating
    /// <see cref="ChatManager"/>.</para><para>Represents the OpenAI API endpoints.</para></summary>
    public OpenAIApis OpenAIApis { get; }

    /// <summary>Creates a new <see cref="ChatManager"/> instance with the
    /// specified <paramref name="options"/>.</summary>
    /// <param name="options">
    /// The <see cref="ChatManagerOptions"/> instance to use for creating
    /// <see cref="ChatManager"/>.
    /// </param>
    /// <returns>A new <see cref="ChatManager"/> instance instantiated with
    /// the specified <paramref name="options"/>.</returns>
    public ChatManager Create(ChatManagerOptions options)
        => new ChatManager(this.OpenAIApis, options);

    /// <summary>Creates a new <see cref="ChatManager"/> instance with the
    /// specified <paramref name="model"/>, <paramref name="temperature"/>,
    /// <paramref name="maxTokens"/>, and <paramref name="numPrompts"/>.</summary>
    /// <param name="model">The <see cref="KnownChatCompletionModel"/> to use
    /// for creating the <see cref="ChatManager"/>.</param>
    /// <param name="temperature">The temperature to use for creating the
    /// <see cref="ChatManager"/>.</param>
    /// <param name="maxTokens"><para>The maximum number of tokens to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents the
    /// maximum number of tokens received for all prompts.</para></param>
    /// <param name="numPrompts"><para>The number of prompts to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents the
    /// number of propmts the API should yield on a request.</para></param>
    public ChatManager Create(KnownChatCompletionModel model, double? temperature = null, int? maxTokens = null, int? numPrompts = null)
        => new ChatManager(this.OpenAIApis, new ChatManagerOptions(model, temperature, maxTokens, numPrompts));
}
