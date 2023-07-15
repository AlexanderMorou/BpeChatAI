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
    public ChatManagerOptions(KnownChatCompletionModel model, double? temperature = null, int? maxTokens = null, int? numPrompts = null)
    {
        this.Model        = model;
        this.Temperature  = temperature;
        this.MaxTokens    = maxTokens;
        this.NumPrompts   = numPrompts;
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
}