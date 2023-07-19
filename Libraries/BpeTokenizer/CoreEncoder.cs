using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

// Converted from tiktokens's lib.rs. Heavily leaned on GPT-4 to do the conversion.
// By large it appears to be a Byte Pair Encoding (BPE) tokenizer and functions as
// designed. As I am not a rust developer, there may be some issues with the conversion.
namespace BpeTokenizer;

internal class BytePairEncodingCore
{
    private Dictionary<byte[], int> encoder;
    private Dictionary<string, int> specialTokensEncoder;
    private Dictionary<int, byte[]> decoder;
    private Dictionary<int, byte[]> specialTokensDecoder;
    public readonly Regex RegexTls;
    public readonly Regex SpecialRegexTls;
    private List<byte[]> sortedTokenBytes;
    private static readonly ByteArrayComparer _commonNativeByteArrayComparer = new ByteArrayComparer();
    public BytePairEncodingCore(Dictionary<byte[], int> encoder, Dictionary<string, int> specialTokensEncoder, Regex regex)
    {
        this.encoder = encoder;
        this.specialTokensEncoder = specialTokensEncoder;

        var specialRegex = new Regex(string.Join("|", specialTokensEncoder.Keys.Select(Regex.Escape)), RegexOptions.Compiled);

        this.decoder = encoder.ToDictionary(x => x.Value, x => x.Key);
        this.specialTokensDecoder = specialTokensEncoder.ToDictionary(x => x.Value, x => Encoding.UTF8.GetBytes(x.Key));

        if (encoder.Count != decoder.Count)
            throw new Exception("Encoder and decoder must be of equal length; maybe you had duplicate token indices in your encoder?");

        this.sortedTokenBytes = encoder.Keys.ToList();
        this.sortedTokenBytes.Sort((x, y) => _commonNativeByteArrayComparer.Compare(x, y));

        this.RegexTls = regex;
        this.SpecialRegexTls = specialRegex;
    }

    private byte[] DecodeNative(Span<int> tokens)
    {
        var ret = new List<byte>();
        foreach (var token in tokens)
        {
            if (this.decoder.TryGetValue(token, out var tokenBytes))
                ret.AddRange(tokenBytes);
            else if (this.specialTokensDecoder.TryGetValue(token, out tokenBytes))
                ret.AddRange(tokenBytes);
            else
                throw new Exception($"Unknown token {token}");
        }
        return ret.ToArray();
    }

    public (List<int> Encoded, int LastPieceTokenLen) EncodeNative(string text, HashSet<string> allowedSpecial)
    {
        var ret = new List<int>();
        var start = 0;
        var lastPieceTokenLen = 0;

        while (true)
        {
            var nextSpecial = this.FindNextSpecial(text, allowedSpecial);
            var end = nextSpecial != null ? nextSpecial.Index : text.Length;

            foreach (Match match in this.RegexTls.Matches(text[start..end]).Cast<Match>())
            {
                var matchBytes = Encoding.UTF8.GetBytes(match.Value);
                if (encoder.TryGetValue(matchBytes, out var token))
                {
                    lastPieceTokenLen = 1;
                    ret.Add(token);
                    continue;
                }

                var tokens = BytePairEncode(matchBytes, encoder);
                lastPieceTokenLen = tokens.Length;
                ret.AddRange(tokens);
            }

            if (nextSpecial == null)
                break;

            var pieceStr = nextSpecial.Value;
            ret.Add(specialTokensEncoder[pieceStr]);
            start = nextSpecial.Index + nextSpecial.Length;
            lastPieceTokenLen = 0;
        }

        return (ret, lastPieceTokenLen);
    }

    private Match? FindNextSpecial(string text, HashSet<string> allowedSpecial)
    {
        if (allowedSpecial.Count == 0)
            return null;
        var matches = this.SpecialRegexTls.Matches(text);
        for (int i = 0; i < matches.Count; i++)
            if (allowedSpecial.Contains(matches[i].Value))
                return matches[i];
        return null;
    }

    public List<int> EncodeOrdinary(string text)
    {
        return EncodeOrdinaryNative(text);
    }

    public List<int> Encode(string text, HashSet<string> allowedSpecial)
        => EncodeNative(text, allowedSpecial).Encoded;

    private List<int> EncodeOrdinaryNative(string text)
    {
        var ret = new List<int>();
        foreach (Match match in this.RegexTls.Matches(text).Cast<Match>())
        {
            var piece = Encoding.UTF8.GetBytes(match.Value);
            if (this.encoder.TryGetValue(piece, out var rank))
            {
                ret.Add(rank);
                continue;
            }
            ret.AddRange(BytePairEncode(piece, this.encoder));
        }
        return ret;
    }

    private static int[] BytePairEncode(byte[] piece, Dictionary<byte[], int> ranks)
    {
        if (piece.Length == 1)
            return new[] { ranks[piece] };
        return BytePairMerge(piece, ranks, p => ranks[piece[p].ToArray()]);
    }

    private static T[] BytePairMerge<T>(byte[] piece, Dictionary<byte[], int> ranks, Func<Range, T> f)
    {
        var parts = Enumerable.Range(0, piece.Length + 1).Select(i => (startIndex: i, rank: int.MaxValue)).ToList();

        int? GetRank(int startIndex, int skip)
        {
            if (startIndex + skip + 2 < parts.Count)
            {
                var range = new Range(parts[startIndex].startIndex, parts[startIndex + skip + 2].startIndex);
                var subPiece = piece[range];
                if (ranks.TryGetValue(subPiece, out var rank))
                    return rank;
            }
            return null;
        }

        for (var i = 0; i < parts.Count - 2; i++)
            if (GetRank(i, 0) is int rank)
                parts[i] = (parts[i].startIndex, rank);

        while (parts.Count > 1)
        {
            (int minRankValue, int minRankIndex) = (int.MaxValue, 0);
            for (var i = 0; i < parts.Count - 1; i++)
                if (parts[i].rank < minRankValue)
                    (minRankValue, minRankIndex) = (parts[i].rank, i);

            if (minRankValue != int.MaxValue)
            {
                parts[minRankIndex] = (parts[minRankIndex].startIndex, GetRank(minRankIndex, 1) ?? int.MaxValue);
                if (minRankIndex > 0)
                    parts[minRankIndex - 1] = (parts[minRankIndex - 1].startIndex, GetRank(minRankIndex - 1, 1) ?? int.MaxValue);
                parts.RemoveAt(minRankIndex + 1);
            }
            else
                break;
        }

        var output = new T[parts.Count - 1];
        for (var i = 0; i < parts.Count - 1; i++)
            output[i] = f(new Range(parts[i].startIndex, parts[i + 1].startIndex));
        return output;
    }

    public (List<int>, int) IncreaseLastPieceTokenLen(List<int> tokens, int lastPieceTokenLen)
    {
        bool TokenIsAllSpace(int token)
        {
            if (decoder.TryGetValue
               (token
               , out var tokenBytes))
            {
                for (int i = 0; i < tokenBytes.Length; i++)
                {
                    char b = (char)tokenBytes[i];
                    if (b == ' ' || b == '\n' || b == '\t')
                        continue;
                    return false;
                }
                return true;
            }
            return false;
        }

        if (lastPieceTokenLen > 0 && TokenIsAllSpace(tokens[^lastPieceTokenLen]))
            while (lastPieceTokenLen < tokens.Count && TokenIsAllSpace(tokens[tokens.Count - lastPieceTokenLen - 1]))
                lastPieceTokenLen++;

        return (tokens, lastPieceTokenLen);
    }

    private static readonly ByteArrayEqualityComparer _commonNativeByteArrayEqualityComparer = new ByteArrayEqualityComparer();
    // I suspect this is a means to generate the seed data to initiate the
    // GPT's token prediction engine. Unconfirmed because likely it would
    // require OpenAI to expose internal behavior that is key to the GPT
    // model's operation.
    public (List<int>, HashSet<int[]>) EncodeUnstableNative(string text, HashSet<string> allowedSpecial)
    {
        var (tokens, lastPieceTokenLen) = EncodeNative(text, allowedSpecial);
        if (lastPieceTokenLen == 0)
        {
            return (tokens, new HashSet<int[]>(new Int32ArrayEqualityComparer()));
        }

        (tokens, lastPieceTokenLen) = IncreaseLastPieceTokenLen(tokens, lastPieceTokenLen);

        var tokensSpan = CollectionsMarshal.AsSpan(tokens);

        var unstableBytes = DecodeNative(tokensSpan.Slice(tokens.Count - lastPieceTokenLen, lastPieceTokenLen));

        tokens.RemoveRange(tokens.Count - lastPieceTokenLen, lastPieceTokenLen);

        var completions = new HashSet<int[]>(new Int32ArrayEqualityComparer());
        if (unstableBytes.Length == 0)
        {
            return (tokens, completions);
        }

        int point = sortedTokenBytes.FindIndex(x => _commonNativeByteArrayEqualityComparer.Equals(x, unstableBytes));

        while (point < sortedTokenBytes.Count && sortedTokenBytes[point].StartsWith(unstableBytes))
        {
            completions.Add(new int[] { encoder[sortedTokenBytes[point]] });
            point++;
        }

        for (int i = 1; i < unstableBytes.Length; i++)
        {
            var prefix = unstableBytes.Take(i).ToArray();
            var suffix = unstableBytes.Skip(i).ToArray();
            point = sortedTokenBytes.FindIndex(x => _commonNativeByteArrayEqualityComparer.Equals(x, suffix));

            while (point < sortedTokenBytes.Count && sortedTokenBytes[point].StartsWith(suffix))
            {
                var possibility = prefix.Concat(sortedTokenBytes[point]).ToArray();
                var encoded = Encoding.UTF8.GetString(possibility);
                var seq = new List<int>();
                var seqLen = 0;

                foreach (var token in EncodeOrdinaryNative(encoded))
                {
                    seq.Add(token);
                    seqLen += decoder[token].Length;
                    if (seqLen >= unstableBytes.Length)
                    {
                        break;
                    }
                }

                completions.Add(seq.ToArray());
                point++;
            }
        }

        if (unstableBytes.Length > 1)
        {
            var decodedString = Encoding.UTF8.GetString(unstableBytes);
            var lastCharacter = decodedString[^1]; // Get the last character

            if (char.IsWhiteSpace(lastCharacter))
            {
                var reencodedPrefix = BytePairEncode(unstableBytes.Take(unstableBytes.Length - 1).ToArray(), encoder);
                var reencodedSuffix = BytePairEncode(unstableBytes.Skip(unstableBytes.Length - 1).ToArray(), encoder);
                var reencoded = reencodedPrefix.Concat(reencodedSuffix).ToArray();

                completions.Add(reencoded);
            }
        }
        return (tokens, completions);
    }
    public (List<int>, List<int[]>) EncodeWithUnstable(string text, HashSet<string> allowedSpecial)
    {
        var (tokens, completions) = EncodeUnstableNative(text, allowedSpecial);
        return (tokens, completions.ToList());
    }

    public int EncodeSingleToken(byte[] piece)
    {
        if (encoder.TryGetValue(piece, out var token))
            return token;
        if (specialTokensEncoder.TryGetValue(Encoding.UTF8.GetString(piece), out token))
            return token;
        throw new KeyNotFoundException();
    }

    public int[] EncodeSinglePiece(byte[] piece)
    {
        if (encoder.TryGetValue(piece, out var token))
            return new[] { token };
        // bytePairEncode implementation is not provided in the original code
        return BytePairEncode(piece, encoder);
    }

    public byte[] DecodeBytes(List<int> tokens)
    {
        using (var memoryStream = new MemoryStream())
        {
            foreach (var token in tokens)
                if (decoder.TryGetValue(token, out var bytes))
                    memoryStream.Write(bytes, 0, bytes.Length);
                else if (specialTokensDecoder.TryGetValue(token, out bytes))
                    memoryStream.Write(bytes, 0, bytes.Length);
            return memoryStream.ToArray();
        }
    }

    public byte[] DecodeSingleTokenBytes(int token)
    {
        if (decoder.TryGetValue(token, out var bytes))
            return bytes;
        if (specialTokensDecoder.TryGetValue(token, out bytes))
            return bytes;
        throw new KeyNotFoundException();
    }
    public List<int> EncodeBytes(byte[] bytes)
    {
        try
        {
            // Attempt to decode the bytes as UTF-8
            string text = Encoding.UTF8.GetString(bytes);
            return EncodeOrdinaryNative(text);
        }
        catch
        {
            // If decoding as UTF-8 fails, encode only the valid part as UTF-8
            int validBytesCount = GetValidUtf8ByteCount(bytes);
            string text = Encoding.UTF8.GetString(bytes, 0, validBytesCount);

            // Call EncodeNative and IncreaseLastPieceTokenLen methods similar to Rust code
            var (tokens, lastPieceTokenLen) = this.EncodeNative(text, new HashSet<string>());

            var (finalTokens, finalLastPieceTokenLen) = IncreaseLastPieceTokenLen(tokens, lastPieceTokenLen);

            // If there are remaining bytes, perform additional logic similar to Rust code
            if (finalTokens.Any() && finalLastPieceTokenLen > 0)
            {
                var tokensSpan = CollectionsMarshal.AsSpan(finalTokens);
                var unstableBytes = DecodeNative(finalTokens.Skip(finalTokens.Count - finalLastPieceTokenLen).ToArray());
                var remainingBytes = bytes[validBytesCount..].ToArray();
                var unstableBytesUpdated = new byte[unstableBytes.Length + remainingBytes.Length];
                unstableBytes.CopyTo(unstableBytesUpdated, 0);
                remainingBytes.CopyTo(unstableBytesUpdated, unstableBytes.Length);
                unstableBytes = unstableBytesUpdated;
                finalTokens.RemoveRange(finalTokens.Count - finalLastPieceTokenLen, finalLastPieceTokenLen);
                finalTokens.AddRange(BytePairEncode(unstableBytes.ToArray(), this.encoder));
            }

            return finalTokens;
        }
    }

    private static int GetValidUtf8ByteCount(byte[] bytes)
    {
        int validBytesCount = 0;
        while (validBytesCount < bytes.Length)
        {
            try
            {
                Encoding.UTF8.GetString(bytes, 0, validBytesCount + 1);
                validBytesCount++;
            }
            catch
            {
                break;
            }
        }
        return validBytesCount;
    }

    public List<byte[]> TokenByteValues()
    {
        return sortedTokenBytes;
    }
}