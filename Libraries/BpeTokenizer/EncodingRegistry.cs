using BpeTokenizer.OpenAI;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BpeTokenizer;
// Simplified conversion of tiktoken's registry.py.
// See: https://github.com/openai/tiktoken/blob/main/tiktoken/registry.py
// I don't bother with several tiktoken extensions, as that is not
// relevant to my use case.

/// <summary>Represents a Byte Pair Encoding (BPE) registry.</summary>
public static class EncodingRegistry
{
    private static readonly object _lock = new object();
    private static readonly Dictionary<string, Encoder> Encodings = new Dictionary<string, Encoder>();

    private static Dictionary<string, Func<Task<EncodingDefinition>>> EncodingConstructors 
        => EncodingDefinitions.EncodingConstructors;

    /// <summary>Gets the <see cref="Encoder"/> for the specified
    /// <paramref name="encodingName"/> asynchronously.</summary>
    /// <param name="encodingName">The name of the encoding to get.</param>
    /// <returns>The <see cref="Encoder"/> for the specified
    /// <paramref name="encodingName"/>.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified
    /// <paramref name="encodingName"/> is not a known encoding.</exception>
    public static async Task<Encoder> GetEncodingAsync(string encodingName)
    {
        if (Encodings.ContainsKey(encodingName))
            return Encodings[encodingName];

        if (!EncodingConstructors.ContainsKey(encodingName))
            throw new ArgumentException($"Unknown encoding {encodingName}");
        // The original included this inside the lock; however, with the
        // use of async, it is illegal to lock during an await.
        // This does cause a low-chance of the same encoding being created
        // twice, but that's not a big deal.
        var constructor = EncodingConstructors[encodingName];
        var constructorDict = await constructor();
        lock (_lock)
        {
            if (Encodings.ContainsKey(encodingName))
                return Encodings[encodingName];

            var enc = 
                new Encoder
                ( constructorDict.Name
                , constructorDict.Regex
                , constructorDict.MergeableRanks
                , constructorDict.SpecialTokens
                , constructorDict.ExplicitNVocab);
            Encodings[encodingName] = enc;
            return enc;
        }
    }

    /// <summary>Returns a list of all known encodings.</summary>
    /// <returns>A list of all known encodings.</returns>
    public static List<string> ListEncodingNames()
    {
        lock (_lock)
            return new List<string>(EncodingConstructors.Keys);
    }
}