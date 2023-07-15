using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BpeTokenizer.Ext;
// May be named `public`; however, the methods within aren't really useful
// outside of this library.
internal static partial class OpenAIPublic
{
    // This was simplified from its original definition which used a loose set of
    // dictionaries instead of the defined TikTokenEncodingDefinition class.
    // Given the focus on making the code type-strict and more appropriate for
    // a C# library. The original code can be found at:
    // https://github.com/openai/tiktoken/blob/main/tiktoken_ext/openai_public.py
    internal const string StartOfText    = "<|startoftext|>";
    internal const string EndOfText      = "<|endoftext|>";
    private  const string FimPrefix      = "<|fim_prefix|>";
    private  const string FimMiddle      = "<|fim_middle|>";
    private  const string FimSuffix      = "<|fim_suffix|>";
    private  const string EndOfPrompt    = "<|endofprompt|>";
    private  const string Gpt2Name       = "gpt2";
    private  const string R50kBaseName   = "r50k_base";
    private  const string P50kBaseName   = "p50k_base";
    private  const string P50kEditName   = "p50k_edit";
    private  const string Cl100kBaseName = "cl100k_base";

    [GeneratedRegex(@"'s|'t|'re|'ve|'m|'ll|'d| ?\p{L}+| ?\p{N}+| ?[^\s\p{L}\p{N}]+|\s+(?!\S)|\s+", RegexOptions.Compiled)]
    private static partial Regex GetCommonPattern();

    [GeneratedRegex(@"(?i:'s|'t|'re|'ve|'m|'ll|'d)|[^\r\n\p{L}\p{N}]?\p{L}+|\p{N}{1,3}| ?[^\s\p{L}\p{N}]+[\r\n]*|\s*[\r\n]+|\s+(?!\S)|\s+", RegexOptions.Compiled)]
    private static partial Regex GetCl100kBasePattern();

    internal static readonly Dictionary<string, Func<Task<TikTokenEncodingDefinition>>> EncodingConstructors =
        new Dictionary<string, Func<Task<TikTokenEncodingDefinition>>>
            { { Gpt2Name      , Gpt2          },
              { R50kBaseName  , R50kBase      },
              { P50kBaseName  , P50kBase      },
              { P50kEditName  , P50kEdit      },
              { Cl100kBaseName, Cl100kBase    } };

    private static async Task<TikTokenEncodingDefinition> Gpt2()
        => new TikTokenEncodingDefinition
           ( Gpt2Name
           , GetCommonPattern()
           , new Dictionary<string, int> { { EndOfText, 50256 } } 
           , await BytePairEncodingLoader.DataGymToMergeableBpeRanksAsync
                   ( vocabBpeFile   : "https://openaipublic.blob.core.windows.net/gpt-2/encodings/main/vocab.bpe"
                   , encoderJsonFile: "https://openaipublic.blob.core.windows.net/gpt-2/encodings/main/encoder.json" )
           , 50257);
    private static async Task<TikTokenEncodingDefinition> R50kBase()
        => new TikTokenEncodingDefinition
               ( R50kBaseName
               , GetCommonPattern()
               , new Dictionary<string, int> { { EndOfText, 50256 } }
               , await BytePairEncodingLoader.LoadTiktokenBpeAsync
                       (tiktokenBpeFile: "https://openaipublic.blob.core.windows.net/encodings/r50k_base.tiktoken")
               , 50257);

    private static async Task<TikTokenEncodingDefinition> P50kBase()
        => new TikTokenEncodingDefinition
               ( P50kBaseName
               , GetCommonPattern()
               , new Dictionary<string, int> { { EndOfText, 50256 } }
               , await BytePairEncodingLoader.LoadTiktokenBpeAsync
                       (tiktokenBpeFile: "https://openaipublic.blob.core.windows.net/encodings/p50k_base.tiktoken")
               , 50281);

    private static async Task<TikTokenEncodingDefinition> P50kEdit()
        => new TikTokenEncodingDefinition
               ( P50kEditName
               , GetCommonPattern()
               , new Dictionary<string, int>
                  { { EndOfText, 50256 },
                    { FimPrefix, 50281 },
                    { FimMiddle, 50282 },
                    { FimSuffix, 50283 } }
               , await BytePairEncodingLoader.LoadTiktokenBpeAsync
                       (tiktokenBpeFile: "https://openaipublic.blob.core.windows.net/encodings/p50k_base.tiktoken"));

    private static async Task<TikTokenEncodingDefinition> Cl100kBase()
        => new TikTokenEncodingDefinition
               ( Cl100kBaseName
               , GetCl100kBasePattern()
               , new Dictionary<string, int>
                   { { EndOfText     , 100257 },
                     { FimPrefix     , 100258 },
                     { FimMiddle     , 100259 },
                     { FimSuffix     , 100260 },
                     { EndOfPrompt   , 100276 } }
               , await BytePairEncodingLoader.LoadTiktokenBpeAsync
                       (tiktokenBpeFile: "https://openaipublic.blob.core.windows.net/encodings/cl100k_base.tiktoken"));
};