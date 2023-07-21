using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpeChatAI.Chat;

/// <summary>Represents the options for creating a <see cref="ChatManager"/>
/// instance.</summary>
public class ChatManagerOptions
{
    /// <summary><para>Initializes a new instance of the <see cref="ChatManagerOptions"/>
    /// with the specified <paramref name="model"/>, <paramref name="temperature"/>,
    /// <paramref name="maxTokens"/>, and <paramref name="numPrompts"/>.</para>
    /// <para> <see langword="null"/> values will not be transmitted during API
    /// calls, which instructs the API to use the default.</para></summary>
    /// <param name="model">The <see cref="KnownChatCompletionModel"/> to use
    /// for creating the <see cref="ChatManager"/>.</param>
    /// <param name="temperature"><para>The temperature to use for creating the
    /// <see cref="ChatManager"/>.</para><para>Represents the randomness of the
    /// output.</para></param>
    /// <param name="maxTokens"><para>The maximum number of tokens to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents the
    /// maximum number of tokens received for all prompts.</para></param>
    /// <param name="numPrompts"><para>The number of prompts to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents the
    /// number of propmts the API should yield on a request.</para></param>
    /// <param name="isModerated"><para>Whether the chat session is moderated.</para>
    /// <para>It's recommended that you set this to <see langword="true"/> for
    /// unstructured chat sessions.</para>
    /// <para>Examples of software that may be okay without moderation:
    /// Personal projects that are not publicly accessible, or chat sessions
    /// that are limited to a small group of people who are aware of the
    /// need to self-filter their commentary.</para>
    /// <para>The decision to use or omit moderation is the responsibility
    /// of the developer using this package.</para></param>
    public ChatManagerOptions
        ( KnownChatCompletionModel model
        , double? temperature   = null
        , int? maxTokens        = null
        , int? numPrompts       = null
        , bool isModerated      = ChatManager.IsModeratedDefault)
    {
        this.Model        = model;
        this.Temperature  = temperature;
        this.MaxTokens    = maxTokens;
        this.NumPrompts   = numPrompts;
        this.IsModerated  = isModerated;
    }

    /// <summary><para>Gets or sets the <see cref="KnownChatCompletionModel"/> to use
    /// for creating the <see cref="ChatManager"/>.</para></summary>
    public KnownChatCompletionModel Model { get; set; }
    /// <summary>
    /// <para>Gets or sets the temperature to use for creating the
    /// <see cref="ChatManager"/>.</para><para>Represents the randomness
    /// of the output.</para></summary>
    public double? Temperature { get; set; }
    /// <summary><para>Gets or sets the maximum number of tokens to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents
    /// the maximum number of tokens received for all prompts.</para></summary>
    public int? MaxTokens { get; set; }
    /// <summary><para>Gets or sets the number of prompts to use for
    /// creating the <see cref="ChatManager"/>.</para><para>Represents
    /// the number of propmts the API should yield on a request.</para></summary>
    public int? NumPrompts { get; set; }
    /// <summary><para>Gets or sets whether the chat session is moderated.</para>
    /// <para>It's recommended that you set this to <see langword="true"/> for
    /// unstructured chat sessions.</para>
    /// <para>Examples of software that may be okay without moderation:
    /// Personal projects that are not publicly accessible, or chat sessions
    /// that are limited to a small group of people who are aware of the
    /// need to self-filter their commentary.</para>
    /// <para>The decision to use or omit moderation is the responsibility
    /// of the developer using this package.</para></summary>
    public bool IsModerated { get; set; }
}