using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace BpeTokenizer.Ext;
/// <summary>Represents a token encoding definition.</summary>
/// <param name="Name"><para>The name of the token encoding definition.</para></param>
/// <param name="Regex"><para>The regular expression that is used to match the tokens.</para></param>
/// <param name="SpecialTokens"><para>The special tokens that are used for the token encoding definition.</para></param>
/// <param name="MergeableRanks"><para>The mergeable ranks that are used for the token encoding definition.</para></param>
/// <param name="ExplicitNVocab"><para>The explicit number of vocabulary that is used for the token encoding definition.</para></param>
internal record TikTokenEncodingDefinition(
    string Name,
    Regex Regex,
    // The dictionaries below should be ReadOnlyDictionary, but there's a slight
    // performance penalty for that.
    Dictionary<string, int> SpecialTokens,
    Dictionary<byte[], int> MergeableRanks,
    int? ExplicitNVocab = null);