using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BpeTokenizer.OpenAI;
using System.Collections.ObjectModel;

// Converted from tiktoken /Load.py
// See https://github.com/openai/tiktoken/blob/main/tiktoken/load.py
namespace BpeTokenizer;

/// <summary>Represents a loader for loading the Byte Pair Encoding (BPE) model.</summary>
public static class Loader
{
    /// <summary>Loads the Byte Pair Encoding (BPE) model from the specified
    /// <paramref name="blobPath"/>.</summary>
    /// <param name="blobPath">The path to the BPE model.</param>
    /// <returns>The loaded BPE model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the specified
    /// <paramref name="blobPath"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exception">Thrown when an error occurred trying to load
    /// the BPE model from the specified <paramref name="blobPath"/>.</exception>
    public static async Task<byte[]> ReadFileAsync(string blobPath)
    {
        if (blobPath is null)
            throw new ArgumentNullException(nameof(blobPath));
        if (!blobPath.StartsWith("http://") && !blobPath.StartsWith("https://"))
            try
            {
                // Assuming BlobFile is a class in C# for handling blob files
                return File.ReadAllBytes(blobPath);
            }
            catch (Exception e)
            {
                throw new Exception(@$"An error occurred trying to load the blob from the {nameof(blobPath)}", e);
            }

        using (var client = new HttpClient())
            return await client.GetByteArrayAsync(blobPath);
    }

    /// <summary>Loads the Byte Pair Encoding (BPE) model from the specified
    /// <paramref name="blobPath"/> and caches it in a relevant cache directory.</summary>
    /// <param name="blobPath">The path to the BPE model.</param>
    /// <returns>The loaded BPE model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the specified
    /// <paramref name="blobPath"/> is <see langword="null"/>.</exception>
    public static async Task<byte[]> ReadFileCachedAsync(string blobPath)
    {
        if (blobPath is null)
            throw new ArgumentNullException(nameof(blobPath));
        string cacheDir;
        if (Environment.GetEnvironmentVariable("TIKTOKEN_CACHE_DIR") is string tiktokenCacheDir)
            cacheDir = tiktokenCacheDir;
        else if (Environment.GetEnvironmentVariable("DATA_GYM_CACHE_DIR") is string dataGymCacheDir)
            cacheDir = dataGymCacheDir;
        else
            cacheDir = Path.Combine(Path.GetTempPath(), "data-gym-cache");

        if (string.IsNullOrEmpty(cacheDir))
            return await ReadFileAsync(blobPath);

        using (var sha1 = SHA1.Create())
        {
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(blobPath));
            var cacheKey = BitConverter.ToString(hash).Replace("-", "").ToLower();

            var cachePath = Path.Combine(cacheDir, cacheKey);
            if (File.Exists(cachePath))
                return File.ReadAllBytes(cachePath);

            var contents = await ReadFileAsync(blobPath);

            Directory.CreateDirectory(cacheDir);
            var tmpFileName = cachePath + "." + Guid.NewGuid() + ".tmp";
            File.WriteAllBytes(tmpFileName, contents);
            File.Move(tmpFileName, cachePath);

            return contents;
        }
    }
    /// <summary>Transforms a Byte Pair Encoding (BPE) vocabulary file and an encoder JSON file into a dictionary
    /// that maps byte sequences to their respective ranks.</summary>
    /// <param name="vocabBpeFile">The path to the BPE vocabulary file.</param>
    /// <param name="encoderJsonFile">The path to the JSON file containing the encoded ranks.</param>
    /// <returns>A dictionary with byte sequences as keys and their respective ranks as values.</returns>
    /// <exception cref="Exception">Throws when the calculated BPE ranks do not match the ones in the encoder JSON file.</exception>
    public static async Task<Dictionary<byte[], int>> DataGymToMergeableBpeRanksAsync(string vocabBpeFile, string encoderJsonFile)
    {
        var rankToIntByte = Enumerable.Range(0, 256).Where(b => ((char)b).IsPyPrintable() && (char)b != ' ').ToList();
        var dataGymByteToByte = rankToIntByte.ToDictionary(b => (char)b, b => b);

        var n = 0;
        for (var b = 0; b < 256; b++)
            if (!rankToIntByte.Contains(b))
            {
                rankToIntByte.Add(b);
                dataGymByteToByte[(char)(256 + n)] = b;
                n++;
            }

        var vocabBpeContents = Encoding.UTF8.GetString(await ReadFileCachedAsync(vocabBpeFile));
        var bpeMerges = vocabBpeContents.Split("\n")[1..^1].Select(mergeStr => mergeStr.Split().ToArray()).ToList();

        byte[] DecodeDataGym(string value) => value.Select(b => (byte)dataGymByteToByte[b]).ToArray();

        var bpeRanks = rankToIntByte.Select((b, i) => new { Key = new byte[] { (byte)b }, Value = i })
            .ToDictionary(x => x.Key, x => x.Value);

        foreach (var merge in bpeMerges)
        {
            var first = merge[0];
            var second = merge[1];
            bpeRanks[DecodeDataGym(first).Concat(DecodeDataGym(second)).ToArray()] = bpeRanks.Count;
        }
        bpeRanks = new Dictionary<byte[], int>(bpeRanks, new ByteArrayEqualityComparer());
        var encoderJson = JsonSerializer.Deserialize<Dictionary<string, int>>(await ReadFileCachedAsync(encoderJsonFile));
        var encoderJsonLoaded = new Dictionary<byte[], int>(encoderJson!.ToDictionary(kvp => DecodeDataGym(kvp.Key), kvp => kvp.Value), new ByteArrayEqualityComparer());
        encoderJsonLoaded.Remove(Encoding.UTF8.GetBytes(EncodingDefinitions.EndOfText));
        encoderJsonLoaded.Remove(Encoding.UTF8.GetBytes(EncodingDefinitions.StartOfText));

        foreach (var key in bpeRanks.Keys)
        {
            if (!encoderJsonLoaded.ContainsKey(key)
                || bpeRanks[key] != encoderJsonLoaded[key])
                throw new Exception("BPE ranks do not match loaded encoder JSON.");
        }


        // Reconstruct the result to use a ByteArrayComparer. Less necessary in
        // python, but required in C#.
        var result = bpeRanks;

        return result;
    }

    /// <summary>Dumps the Byte Pair Encoding (BPE) ranks to the specified <paramref name="tiktokenBpeFile"/>.</summary>
    /// <param name="bpeRanks">The dictionary with byte sequences as keys and their respective ranks as values.</param>
    /// <param name="tiktokenBpeFile">The path to the file where the BPE ranks should be dumped to.</param>
    /// <exception cref="Exception">Throws when the BPE ranks could not be dumped to the specified <paramref name="tiktokenBpeFile"/>.</exception>
    public static void DumpTiktokenBpeAsync(Dictionary<byte[], int> bpeRanks, string tiktokenBpeFile)
    {
        try
        {
            using (var f = new FileStream(tiktokenBpeFile, FileMode.Create))
                foreach (var (token, rank) in bpeRanks.OrderBy(x => x.Value))
                {
                    var line = Convert.ToBase64String(token) + " " + rank + "\n";
                    var bytes = Encoding.UTF8.GetBytes(line);
                    f.Write(bytes, 0, bytes.Length);
                }
        }
        catch (Exception e)
        {
            throw new Exception("Failed to dump BPE ranks to file.", e);
        }
    }

    /// <summary>Loads the Byte Pair Encoding ranks from a file.</summary><param name="tiktokenBpeFile">The url to the BPE file.</param><returns>A dictionary mapping byte arrays to their BPE rank.</returns>
    public static async Task<Dictionary<byte[], int>> LoadTiktokenBpeAsync(string tiktokenBpeFile)
    {
        // Note: do not add caching to this function
        var contents = await ReadFileCachedAsync(tiktokenBpeFile);
        var lines = Encoding.UTF8.GetString(contents).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        var result = lines.Select
            (line =>
             {
                 var parts = line.Split();
                 return new { Key = Convert.FromBase64String(parts[0]), Value = int.Parse(parts[1]) };
             }).ToDictionary(x => x.Key, x => x.Value);
        // Reconstruct the result to use a ByteArrayComparer. Less necessary in
        // python, but required in C#.
        return new Dictionary<byte[], int>(result, new ByteArrayEqualityComparer());
    }
}
