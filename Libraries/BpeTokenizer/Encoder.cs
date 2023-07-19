using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static BpeTokenizer.Core;
using BpeTokenizer.OpenAI;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
// Converted from tiktokens /core.py
// See: https://github.com/openai/tiktoken/blob/main/tiktoken/core.py
// numThreads is set to 4 by default vs 8 because of the overhead
// incurred by the TPL. From my testing, anything beyond 4 threads
// starts to fall behind the four-threaded version.
// Python does not have this issue.
namespace BpeTokenizer;

/// <summary>
/// Represnts a Byte Pair Encoder tokenizer.
/// </summary>
public class Encoder
{
    /// <summary><para>Set to 4 by default vs 8 because of the overhead
    /// incurred by the TPL. From my testing, anything beyond 4 threads
    /// starts to fall behind the four-threaded version.</para><para>
    /// Python does not have this issue.</para></summary>
    private const int numThreadsDefault = 4;
    private readonly Regex _regex;
    private readonly Dictionary<string, int> _specialTokens;
    private readonly BytePairEncodingCore _coreBpe;

    /// <summary>Gets the name of the BytePairEncoder.</summary>
    public string Name { get; }
    /// <summary>Gets the maximum token value.</summary>
    public int MaxTokenValue { get; }

    /// <summary>Initializes a new instance of the <see cref="Encoder"/>
    /// class with the specified <paramref name="name"/>, <paramref name="regex"/>,
    /// <paramref name="mergeableRanks"/>, <paramref name="specialTokens"/>,
    /// and <paramref name="explicitNVocab"/>.</summary>
    /// <param name="name">The name of the BytePairEncoder.</param>
    /// <param name="regex">The regular expression used to match tokens.</param>
    /// <param name="mergeableRanks">The mergeable ranks.</param>
    /// <param name="specialTokens">The special tokens.</param>
    /// <param name="explicitNVocab">The explicit number of vocabulary.</param>
    /// <exception cref="ArgumentException">Number of mergeable tokens and special tokens must be equal to <paramref name="explicitNVocab"/>.</exception>
    public Encoder
        ( string name
        , Regex regex
        , Dictionary<byte[], int> mergeableRanks
        , Dictionary<string, int> specialTokens
        , int? explicitNVocab = null)
    {
        // eliminated the _mergeableRanks field because it's not used anywhere
        // other than the constructor.
        // ToDo: Keep tabs on the original implementation and if that changes,
        //       update this accordingly.
        this.Name = name;
        this._regex = regex;
        this._specialTokens = specialTokens;

        this.MaxTokenValue = Math.Max(mergeableRanks.Values.Max(), specialTokens.Values.DefaultIfEmpty(0).Max());
        if (explicitNVocab.HasValue
            && (mergeableRanks.Count + specialTokens.Count != explicitNVocab.Value
                || this.MaxTokenValue != explicitNVocab - 1))
            throw new ArgumentException($"Number of mergeable tokens and special tokens must be equal to {nameof(explicitNVocab)}.");
        this._coreBpe = new BytePairEncodingCore(mergeableRanks, specialTokens, regex);
    }

    /// <summary>Returns a string that represents the current object.</summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
        => $"<Encoding {this.Name}>";

    /// <summary>Encodes the specified <paramref name="text"/> into a list of integers.</summary>
    public List<int> EncodeOrdinary(string text)
    {
        try
        {
            return this._coreBpe.EncodeOrdinary(text);
        }
        catch (EncoderFallbackException)
        {
            var convertedBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(text));
            text = Encoding.UTF8.GetString(convertedBytes);
            return this._coreBpe.EncodeOrdinary(text);
        }
    }

    /// <summary>Encodes the specified <paramref name="text"/> into a list of integers
    /// using the specified <paramref name="allowedSpecial"/> and <paramref name="disallowedSpecial"/>.</summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="allowedSpecial">The allowed special tokens.</param>
    /// <param name="disallowedSpecial">The disallowed special tokens.</param>
    /// <returns>A list of integers.</returns>
    /// <exception cref="ArgumentException">The <paramref name="disallowedSpecial"/> contains a special token that is not in the <paramref name="allowedSpecial"/>.</exception>
    public List<int> Encode(string text, HashSet<string>? allowedSpecial = null, ICollection<string>? disallowedSpecial = null)
    {
        allowedSpecial ??= new HashSet<string>();

        // ToDo: Keep an eye on the original implementation of `encode` from 
        //       the python version. If they change disallowedSpecial, when
        //       null, to contain something other than "all", then we'll need
        //       to update this accordingly.
        if (disallowedSpecial == null
            || disallowedSpecial.Contains("all"))
            disallowedSpecial = SpecialTokensSet.Except(allowedSpecial).ToList();

        if (disallowedSpecial.Any())
        {
            var disallowedSpecialSet = new HashSet<string>(disallowedSpecial);
            var match = SpecialTokenRegex(disallowedSpecialSet).Match(text);
            if (match.Success)
                RaiseDisallowedSpecialToken(match.Value);
        }

        try
        {
            return this._coreBpe.Encode(text, allowedSpecial);
        }
        catch (EncoderFallbackException)
        {
            var convertedBytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, Encoding.Unicode.GetBytes(text));
            text = Encoding.UTF8.GetString(convertedBytes);
            return this._coreBpe.Encode(text, allowedSpecial);
        }
    }

    /// <summary>Encodes the specified <paramref name="text"/> into a list of integers
    /// using the specified <paramref name="numThreads"/>.</summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="numThreads">The number of threads to use to encode the batch.</param>
    /// <returns>A list of integers.</returns>
    public List<int>[] EncodeOrdinaryBatch(List<string> text, int numThreads = numThreadsDefault)
    {
        var result = new ConcurrentDictionary<int, List<int>>();
        Parallel.ForEach(Enumerable.Range(0, text.Count), new ParallelOptions { MaxDegreeOfParallelism = numThreads },
            i => result[i] = EncodeOrdinary(text[i]));
        return result.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
    }

    /// <summary>Encodes the specified <paramref name="text"/> into a list of integers
    /// using the specified <paramref name="numThreads"/>, <paramref name="allowedSpecial"/>,
    /// and <paramref name="disallowedSpecial"/>.</summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="numThreads">The number of threads to use to encode the batch.</param>
    /// <param name="allowedSpecial">The allowed special tokens.</param>
    /// <param name="disallowedSpecial">The disallowed special tokens.</param>
    /// <returns>A list of integers.</returns>
    /// <exception cref="ArgumentException">The <paramref name="disallowedSpecial"/> contains a special token that is not in the <paramref name="allowedSpecial"/>.</exception>
    public List<List<int>> EncodeBatch
        ( List<string> text
        , int numThreads = numThreadsDefault
        , HashSet<string>? allowedSpecial = null
        , ICollection<string>? disallowedSpecial = null)
    {
        allowedSpecial ??= new HashSet<string>();
        // ToDo: Keep an eye on the original implementation of `encode_batch` 
        //       from the python version. If they change disallowedSpecial,
        //       when null, to contain something other than "all", then we'll
        //       need to update this accordingly.
        if (disallowedSpecial == null
            || disallowedSpecial.Contains("all"))
            disallowedSpecial = SpecialTokensSet.Except(allowedSpecial).ToList();

        var disallowedSpecialSet = new HashSet<string>(disallowedSpecial);

        List<int>[] result = new List<int>[text.Count];

        Parallel.For
            (0
            , text.Count
            , new ParallelOptions { MaxDegreeOfParallelism = numThreads }
            , i => result[i] = Encode(text[i], allowedSpecial, disallowedSpecialSet));
        return result.ToList();
    }

    /// <summary>Encodes the specified <paramref name="text"/> into a list of integers
    /// using the specified <paramref name="allowedSpecial"/> and <paramref name="disallowedSpecial"/>.</summary>
    /// <param name="text">The text to encode.</param>
    /// <param name="allowedSpecial">The allowed special tokens.</param>
    /// <param name="disallowedSpecial">The disallowed special tokens.</param>
    /// <returns>A list of integers.</returns>
    /// <exception cref="ArgumentException">The <paramref name="disallowedSpecial"/> contains a special token that is not in the <paramref name="allowedSpecial"/>.</exception>
    public (List<int> StableTokens, List<int[]> Completions)
        EncodeWithUnstable
        (string text
        , HashSet<string>? allowedSpecial = null
        , ICollection<string>? disallowedSpecial = null)
    {
        allowedSpecial ??= new HashSet<string>();
        // ToDo: Keep an eye on the original implementation of `encode_with_unstable`
        //       from the python version. If they change disallowedSpecial, when null,
        //       to contain something other than "all", then we'll need to update
        //       this accordingly.
        if (disallowedSpecial == null
            || disallowedSpecial.Contains("all"))
            disallowedSpecial = SpecialTokensSet.Except(allowedSpecial).ToList();

        var disallowedSpecialSet = new HashSet<string>(disallowedSpecial);
        if (disallowedSpecialSet.Any())
        {
            var match = SpecialTokenRegex(disallowedSpecialSet).Match(text);
            if (match.Success)
                RaiseDisallowedSpecialToken(match.Value);
        }

        return this._coreBpe.EncodeWithUnstable(text, allowedSpecial);
    }

    /// <summary>Encodes the <paramref name="textOrBytes"/> into a single token.</summary>
    /// <param name="textOrBytes">The text or bytes to encode.</param>
    /// <returns>The encoded token.</returns>
    /// <exception cref="InvalidCastException">The <paramref name="textOrBytes"/> is not a string or byte[].</exception>
    public int EncodeSingleToken(object textOrBytes)
    {
        if (textOrBytes is string text)
            textOrBytes = Encoding.UTF8.GetBytes(text);

        return this._coreBpe.EncodeSingleToken((byte[])textOrBytes);
    }

    /// <summary>Decodes the <paramref name="tokens"/> into a byte array.</summary>
    /// <param name="tokens">The tokens to decode.</param>
    public byte[] DecodeBytes(List<int> tokens)
        => this._coreBpe.DecodeBytes(tokens);

    /// <summary>Decodes the <paramref name="tokens"/> into a string
    /// using the specified <paramref name="errors"/> approach for handling
    /// decoding errors.</summary>
    /// <param name="tokens">The tokens to decode.</param>
    /// <param name="errors">The approach to use for handling decoding errors.</param>
    /// <returns>The decoded string.</returns>
    /// <exception cref="DecoderFallbackException">The <paramref name="errors"/> is
    /// <see cref="DecodeErrorApproach.Strict"/> and a decoding error occurs.</exception>
    public string Decode(List<int> tokens, DecodeErrorApproach errors = DecodeErrorApproach.Replace)
    {
        // Convert tokens back into bytes
        byte[] bytes = _coreBpe.DecodeBytes(tokens);

        // Create UTF8 decoder
        Decoder decoder = Encoding.UTF8.GetDecoder();

        switch (errors)
        {
            case DecodeErrorApproach.Ignore:
                decoder.Fallback = new DecoderReplacementFallback(string.Empty);
                break;
            case DecodeErrorApproach.Strict:
                // For 'strict' we use the exception fallback which will
                // throw an exception on error.
                decoder.Fallback = DecoderFallback.ExceptionFallback;
                break;
            default:
                decoder.Fallback = new DecoderReplacementFallback("�");
                break;
        }
        // Decode bytes into string
        char[] chars = new char[decoder.GetCharCount(bytes, 0, bytes.Length)];
        decoder.GetChars(bytes, 0, bytes.Length, chars, 0);

        return new string(chars);
    }

    /// <summary>Decodes the <paramref name="token"/> into a byte array.</summary>
    /// <param name="token">The token to decode.</param>
    /// <returns>The decoded byte array.</returns>
    public byte[] DecodeSingleTokenBytes(int token)
        => this._coreBpe.DecodeSingleTokenBytes(token);

    /// <summary>Decodes the <paramref name="tokens"/> into a set of byte arrays.
    /// This is a convenience method for calling <see cref="DecodeSingleTokenBytes(int)"/>
    /// on each token in the <paramref name="tokens"/>.</summary>
    /// <param name="tokens">The tokens to decode.</param>
    /// <returns>The decoded byte arrays.</returns>
    public List<byte[]> DecodeTokensBytes(List<int> tokens)
        => tokens.Select(token => DecodeSingleTokenBytes(token)).ToList();

    /// <summary>Decodes the <paramref name="tokens"/> into a string and
    /// offsets for each token.</summary>
    /// <param name="tokens">The tokens to decode.</param>
    /// <returns>The decoded string and offsets.</returns>
    public (string Text, List<int> Offsets) DecodeWithOffsets(List<int> tokens)
    {
        var tokenBytes = DecodeTokensBytes(tokens);

        int textLen = 0;
        var offsets = new List<int>();
        foreach (var token in tokenBytes)
        {
            offsets.Add(Math.Max(0, textLen - (0x80 <= token[0] && token[0] < 0xC0 ? 1 : 0)));
            textLen += token.Count(c => !(0x80 <= c && c < 0xC0));
        }

        var text = Encoding.UTF8.GetString(tokenBytes.SelectMany(b => b).ToArray());
        return (text, offsets);
    }

    /// <summary><para>Decodes the <paramref name="tokens"/> into a string and ranges for
    /// each token.</para><para>Each range is effectively a tuple of (start, length)
    /// where start is the index of the first character of the token in the
    /// decoded string and length is the number of characters in the token.</para></summary>
    /// <param name="tokens"><para>The tokens to decode.</para></param>
    /// <returns><para>The decoded string and ranges.</para></returns>
    /// <remarks><para>GPT-4 wrote this function's first version as an alternative
    /// to DecodeWithOffsets.</para><para>Given that C# strings are UTF-16, I'm not
    /// sure how the DecodeWithOffsets would be useful?</para><para>Method below is
    /// untested. Might be useful for visualization?</para></remarks>
    public (string Text, List<Range> Ranges) DecodeWithRanges(List<int> tokens)
    {
        var tokenBytes = DecodeTokensBytes(tokens);

        var ranges = new List<Range>();
        var textBuilder = new StringBuilder();

        foreach (var token in tokenBytes)
        {
            // Convert token bytes to UTF-16 string.
            var tokenString = Encoding.UTF8.GetString(token);

            // Add range of this token.
            ranges.Add(new Range(textBuilder.Length, textBuilder.Length + tokenString.Length));

            // Append token string to full text.
            textBuilder.Append(tokenString);
        }

        var text = textBuilder.ToString();
        return (text, ranges);
    }

    /// <summary>Decodes a batch of token lists into a list of strings.</summary>
    /// <param name="batch">The batch of token lists to decode.</param>
    /// <param name="errors"><para>The approach to use for handling decoding errors.</para></param>
    /// <param name="numThreads"><para>The number of threads to use for decoding.</para></param>
    /// <returns><para>The decoded strings.</para></returns>
    public List<string> DecodeBatch(List<List<int>> batch, DecodeErrorApproach errors = DecodeErrorApproach.Replace, int numThreads = numThreadsDefault)
    {
        var result = new ConcurrentDictionary<int, string>();
        Parallel.ForEach
            ( Enumerable.Range(0, batch.Count)
            , new ParallelOptions 
              { MaxDegreeOfParallelism = numThreads }
            , i => result[i] = Decode(batch[i], errors));
        return result.OrderBy(kvp => kvp.Key)
               .Select(kvp => kvp.Value)
               .ToList();
    }

    /// <summary>Decodes a batch of token lists into a list of byte arrays representing each token.</summary>
    /// <param name="batch">The batch of token lists to decode.</param>
    /// <param name="numThreads"><para>The number of threads to use for decoding.</para></param>
    /// <returns><para>The decoded strings and offsets.</para></returns>
    public List<byte[]> DecodeBytesBatch(List<List<int>> batch, int numThreads = numThreadsDefault)
    {
        var result = new ConcurrentDictionary<int, byte[]>();
        Parallel.ForEach
            ( batch
              .Select((value, index) => new { index, value })
            , new ParallelOptions 
              { MaxDegreeOfParallelism = numThreads }
            , tokenBatch
              =>
              {
                  var decodedBytes = DecodeBytes(tokenBatch.value);
                  result[tokenBatch.index] = decodedBytes;
              });
        return result
               .OrderBy(kvp => kvp.Key)
               .Select(kvp => kvp.Value)
               .ToList();
    }

    /// <summary>The byte arrays for all tokens in the vocabulary.</summary>
    public List<byte[]> TokenByteValues()
        => this._coreBpe.TokenByteValues();

    /// <summary>The <see cref="int"/> denoting the index of the end-of-text token.</summary>
    public int EotToken => this._specialTokens[EncodingDefinitions.EndOfText];

    /// <summary>The <see cref="HashSet{T}"/> of <see cref="string"/> representing all special tokens.</summary>
    public HashSet<string> SpecialTokensSet
        => new HashSet<string>(this._specialTokens.Keys);

    /// <summary>The <see cref="int"/> denoting the number of tokens in the vocabulary.</summary>
    public int NVocab => this.MaxTokenValue + 1;

    /// <summary>Encodes a single piece of text or bytes.</summary>
    /// <param name="textOrBytes"><para>The text or bytes to encode.</para></param>
    /// <returns>An array of <see cref="int"/> denoting the tokenized text.</returns>
    /// <exception cref="InvalidCastException"><para>Thrown if <paramref name="textOrBytes"/>
    /// is neither a <see cref="string"/> nor a <see cref="byte"/> array.</para></exception>
    public int[] EncodeSinglePiece(object textOrBytes)
    {
        if (textOrBytes is string text)
            textOrBytes = Encoding.UTF8.GetBytes(text);

        return this._coreBpe.EncodeSinglePiece((byte[])textOrBytes);
    }

    /// <summary>Encodes the <paramref name="text"/> into a list of tokens.</summary>
    /// <param name="text"><para>The text to encode.</para></param>
    /// <returns><para>The list of tokens.</para>
    /// <para>Each token is an <see cref="int"/> denoting the tokenized text.</para></returns>
    public int[] EncodeOnlyNativeBpe(string text)
    {
        var ret = new List<int>();
        foreach (Match piece in this._regex.Matches(text).Cast<Match>())
            ret.AddRange(this._coreBpe.EncodeSinglePiece(Encoding.UTF8.GetBytes(piece.Value)));
        return ret.ToArray();
    }

    /// <summary>Encodes the <paramref name="text"/> into a list of tokens
    /// with the <paramref name="allowedSpecial"/> and <paramref name="disallowedSpecial"/>.</summary>
    /// <param name="text"><para>The text to encode.</para></param>
    /// <param name="allowedSpecial"><para>The set of allowed special tokens.</para></param>
    /// <param name="disallowedSpecial"><para>The set of disallowed special tokens.</para></param>
    /// <returns><para>The list of tokens.</para>
    /// <para>Each token is an <see cref="int"/> denoting the tokenized text.</para></returns>
    /// <exception cref="ArgumentException"><para>Thrown if <paramref name="text"/> contains
    /// a special token that is not in <paramref name="allowedSpecial"/>
    /// or is present in the <paramref name="disallowedSpecial"/>.</para></exception>
    public int CountTokens(string text, HashSet<string>? allowedSpecial = null, ICollection<string>? disallowedSpecial = null)
        // ToDo: Look into the NuGet package TikToken for inspiration
        // to a more efficient implementation.
        => this.Encode(text, allowedSpecial, disallowedSpecial).Count;

    /// <summary>Encodes the <paramref name="text"/> (as a byte array) into a list of tokens.</summary>
    /// <param name="text"><para>The <see cref="byte"/> array to encode.</para></param>
    /// <returns><para>The list of tokens.</para>
    /// <para>Each token is an <see cref="int"/> denoting the tokenized text.</para></returns>
    public List<int> EncodeBytes(byte[] text)
        => this._coreBpe.EncodeBytes(text);
}

internal static class Core
{
    public static Regex SpecialTokenRegex(HashSet<string> tokens)
    {
        var inner = string.Join("|", tokens.Select(token => Regex.Escape(token)));
        return new Regex($"({inner})", RegexOptions.Compiled);
    }

    public static void RaiseDisallowedSpecialToken(string token)
        => throw new ArgumentException
                 ($"Encountered text corresponding to disallowed special token {token}.\n"
                 + $"If you want this text to be encoded as a special token, pass it to `allowed_special`, e.g. `allowed_special={{{token}, ...}}`.\n"
                 + $"If you want this text to be encoded as normal text, disable the check for this token by passing `disallowed_special=(enc.special_tokens_set - {{{token}}})`.\n"
                 + "To disable this check for all special tokens, pass `disallowed_special=()`.\n");
}

/// <summary>A class for comparing byte arrays.</summary>
public class ByteArrayEqualityComparer
    : IEqualityComparer<byte[]>
{
    /// <summary>Returns whether the two byte arrays are equal.</summary>
    /// <param name="x"><para>The first byte array.</para></param>
    /// <param name="y"><para>The second byte array.</para></param>
    /// <returns><para>Whether the two byte arrays are equal.</para></returns>
    /// <remarks><para>Two byte arrays are equal if they have the same length and
    /// the same bytes at the same positions.</para></remarks>
    public bool Equals(byte[]? x, byte[]? y)
    {
        if (x == null || y == null)
            return x == y;
        if (x.Length != y.Length)
            return false;
        return x.StartsWith(y);
    }

    /// <summary>Returns the hash code of the byte array.</summary>
    /// <param name="obj"><para>The byte array.</para></param>
    /// <returns><para>The hash code of the byte array.</para></returns>
    /// <remarks><para>The hash code is computed by sampling the byte array.</para></remarks>
    /// <seealso cref="ByteArrayEqualityComparer.Equals(byte[], byte[])"/>
    public int GetHashCode(byte[] obj)
    {
        const int samplingDenominator = 8;

        var stepSize = Math.Max(1, obj.Length / samplingDenominator);

        int hash = 47;
        for (int i = 0; i < obj.Length; i += stepSize)
            hash = hash * 61 + obj[i];
        return hash;
    }
}

/// <summary>A class for comparing byte arrays.</summary>
/// <remarks><para>Two byte arrays are compared by comparing their lengths and
/// then comparing their bytes at the same positions.</para>
/// <para>Useful for sorting lists of byte arrays.</para></remarks>
public class ByteArrayComparer
    : IComparer<byte[]>
{
    /// <summary>Compares two byte arrays.</summary>
    /// <param name="x"><para>The first byte array.</para></param>
    /// <param name="y"><para>The second byte array.</para></param>
    /// <returns><para>The result of the comparison.</para></returns>
    /// <remarks><para>Two byte arrays are compared by comparing their lengths and
    /// then comparing their bytes at the same positions.</para></remarks>
    public unsafe int Compare(byte[]? x, byte[]? y)
    {
        if (x == null)
            return y == null ? 0 : -1;
        if (y == null)
            return 1;

        var len = Math.Min(x.Length, y.Length);

        switch (len)
        {
            case 0:
                return x.Length.CompareTo(y.Length);
            case 1:
                return x[0] != y[0] ? x[0].CompareTo(y[0]) : x.Length.CompareTo(y.Length);
            case 2:
                if (Unsafe.ReadUnaligned<ushort>(ref x[0]) == Unsafe.ReadUnaligned<ushort>(ref y[0]))
                    return x.Length.CompareTo(y.Length);
                return x[0] != y[0] ? x[0].CompareTo(y[0]) : x[1].CompareTo(y[1]);
            default:
                var intLen = len / 4;
                var byteRemainder = len % 4;
                var byteRemainderOffset = len - byteRemainder;
                if (intLen > 0)
                    fixed (byte* xPtr = x, yPtr = y)
                    {
                        int* xIntPtr = (int*)xPtr;
                        int* yIntPtr = (int*)yPtr;
                        for (int i = 0; i < intLen; i++, xIntPtr++, yIntPtr++)
                        {
                            if (*xIntPtr != *yIntPtr)
                            {
                                // Compare byte by byte in the differing int
                                for (int j = 0; j < 4; j++)
                                {
                                    var xi = x[i * 4 + j];
                                    var yi = y[i * 4 + j];
                                    if (xi != yi)
                                        return xi.CompareTo(yi);
                                }
                            }
                        }
                    }
                for (int i = byteRemainderOffset; i < len; i++)
                {
                    if (x[i] != y[i])
                        return x[i].CompareTo(y[i]);
                }
                return x.Length.CompareTo(y.Length); // If all preceding bytes are equal, the longer array is considered greater
        }
    }
}

/// <summary>A class for comparing int arrays.</summary>
/// <remarks><para>Two int arrays are compared by comparing their lengths and
/// then comparing their ints at the same positions.</para></remarks>
public class Int32ArrayEqualityComparer
    : IEqualityComparer<int[]>
{
    /// <summary>Returns whether the two int arrays are equal.</summary>
    /// <param name="x"><para>The first int array.</para></param>
    /// <param name="y"><para>The second int array.</para></param>
    /// <returns><para>Whether the two int arrays are equal.</para></returns>
    /// <remarks><para>Two int arrays are equal if they have the same length and
    /// the same ints at the same positions.</para></remarks>
    public bool Equals(int[]? x, int[]? y)
    {
        if (x == null || y == null)
            return x == y;
        if (x.Length != y.Length)
            return false;
        for (int i = 0; i < x.Length; i++)
        {
            if (x[i] != y[i])
                return false;
        }
        return true;
    }

    /// <summary>Returns the hash code of the int array.</summary>
    /// <param name="obj"><para>The int array.</para></param>
    /// <returns><para>The hash code of the int array.</para></returns>
    /// <remarks><para>The hash code is computed by sampling the int array.</para></remarks>
    /// <seealso cref="Int32ArrayEqualityComparer.Equals(int[], int[])"/>
    public int GetHashCode(int[] obj)
    {
        const int samplingDenominator = 16;

        var stepSize = Math.Max(1, obj.Length / samplingDenominator);

        int hash = 47;
        for (int i = 0; i < obj.Length; i += stepSize)
        {
            hash = hash * 61 + obj[i];
        }
        return hash;
    }
}

/// <summary>Specifies how to handle decoding errors.</summary>
public enum DecodeErrorApproach
{
    /// <summary>Replace the invalid bytes with the replacement character (U+FFFD).</summary>
    Replace,
    /// <summary>Ignore the invalid bytes.</summary>
    Ignore,
    /// <summary>Throw an exception.</summary>
    Strict
}