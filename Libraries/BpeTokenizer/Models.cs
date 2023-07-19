using System.Diagnostics;

using static BpeTokenizer.EncodingRegistry;
// Converted from tiktoken /model.py
// See https://github.com/openai/tiktoken/blob/main/tiktoken/model.py

namespace BpeTokenizer;

/// <summary>This class contains the model names and their corresponding encodings.</summary>
public class Models
{
    // TODO: The original source says these will likely be replaced by an API endpoint.
    // Once that happens, we can remove these dictionaries.
    /// <summary>Maps a model name to its encoding.</summary>
    public readonly static Dictionary<string, string> ModelPrefixToEncoding = 
          // chat
        new Dictionary<string, string>
        { { "gpt-4-"                       , "cl100k_base"  }    // e.g., gpt-4-0314, etc., plus gpt-4-32k
        , { "gpt-3.5-turbo-"               , "cl100k_base"  }    // e.g, gpt-3.5-turbo-0301, -0401, etc.
        , { "gpt-35-turbo"                 , "cl100k_base"  } }; // Azure deployment name

    /// <summary>Maps a model name to its encoding.</summary>
    public readonly static Dictionary<string, string> ModelToEncoding = 
        new Dictionary<string, string>
          // chat
        { { "gpt-4"                        , "cl100k_base"  }
        , { "gpt-3.5-turbo"                , "cl100k_base"  }
        , { "gpt-35-turbo"                 , "cl100k_base"  } // Azure deployment name  
          // Future use (available through the API on Jan 4, 2024)
        , { "ada-002"                      , "cl100k_base"  }
        , { "babbage-002"                  , "cl100k_base"  }
        , { "curie-002"                    , "cl100k_base"  }
        , { "davinci-002"                  , "cl100k_base"  }
        , { "gpt-3.5-turbo-instruct"       , "cl100k_base"  }
          // code
        , { "code-davinci-002"             , "p50k_base"    }
        , { "code-davinci-001"             , "p50k_base"    }
        , { "code-cushman-002"             , "p50k_base"    }
        , { "code-cushman-001"             , "p50k_base"    }
        , { "davinci-codex"                , "p50k_base"    }
        , { "cushman-codex"                , "p50k_base"    }
          // edit
        , { "text-davinci-edit-001"        , "p50k_edit"    }
        , { "code-davinci-edit-001"        , "p50k_edit"    }
          // embeddings
        , { "text-embedding-ada-002"       , "cl100k_base"  }
          // Legacy (no longer available through the API on Jan 4, 2024)
        , { "text-davinci-003"             , "p50k_base"    }
        , { "text-davinci-002"             , "p50k_base"    }
        , { "text-davinci-001"             , "r50k_base"    }
        , { "text-curie-001"               , "r50k_base"    }
        , { "text-babbage-001"             , "r50k_base"    }
        , { "text-ada-001"                 , "r50k_base"    }
        , { "davinci"                      , "r50k_base"    }
        , { "curie"                        , "r50k_base"    }
        , { "babbage"                      , "r50k_base"    }
        , { "ada"                          , "r50k_base"    }
          // old embeddings
        , { "text-similarity-davinci-001"  , "r50k_base"    }
        , { "text-similarity-curie-001"    , "r50k_base"    }
        , { "text-similarity-babbage-001"  , "r50k_base"    }
        , { "text-similarity-ada-001"      , "r50k_base"    }
        , { "text-search-davinci-doc-001"  , "r50k_base"    }
        , { "text-search-curie-doc-001"    , "r50k_base"    }
        , { "text-search-babbage-doc-001"  , "r50k_base"    }
        , { "text-search-ada-doc-001"      , "r50k_base"    }
        , { "code-search-babbage-code-001" , "r50k_base"    }
        , { "code-search-ada-code-001"     , "r50k_base"    }
          // open source
        , { "gpt2"                         , "gpt2"         } };

    private static readonly List<string> _modelSearchOrder = new List<string>
        // chat
        { "gpt-4"
        // Dictionaries are somewhat similar to HashSet; which means we need
        // to define a deterministic order for the keys. Otherwise, the order
        // of the keys is not guaranteed. We don't know if gpt-3.5-turbo-instruct
        // will always map to the same encoding.
        , "gpt-3.5-turbo-instruct"
        , "gpt-3.5-turbo"
        , "gpt-35-turbo"
        // Future use (available through the API on Jan 4, 2024)
        , "ada-002"
        , "babbage-002"
        , "curie-002"
        , "davinci-002"
        // code
        , "code-davinci-002"
        , "code-davinci-001"
        , "code-cushman-002"
        , "code-cushman-001"
        , "davinci-codex"
        , "cushman-codex"
        // edit
        , "text-davinci-edit-001"
        , "code-davinci-edit-001"
        // embeddings
        , "text-embedding-ada-002"
        // Legacy (no longer available through the API on Jan 4, 2024)
        , "text-davinci-003"
        , "text-davinci-002"
        , "text-davinci-001"
        , "text-curie-001"
        , "text-babbage-001"
        , "text-ada-001"
        , "davinci"
        , "curie"
        , "babbage"
        , "ada"
        // old embeddings
        , "text-similarity-davinci-001"
        , "text-similarity-curie-001"
        , "text-similarity-babbage-001"
        , "text-similarity-ada-001"
        , "text-search-davinci-doc-001"
        , "text-search-curie-doc-001"
        , "text-search-babbage-doc-001"
        , "text-search-ada-doc-001"
        , "code-search-babbage-code-001"
        , "code-search-ada-code-001"
        // open source
        , "gpt2" };

    // Ensure that all of the _modelSearchOrder keys exist in the ModelToEncoding
    // dictionary. This is a sanity check to ensure that we don't have a typo in
    // the _modelSearchOrder list, and that they don't get out of sync.
    static Models()
        => Debug.Assert(ModelToEncoding.Count == _modelSearchOrder.Count
                        && _modelSearchOrder.All(model => ModelToEncoding.ContainsKey(model)));

    /// <summary>Gets the encoding for the <paramref name="modelName"/> provided.</summary>
    /// <param name="modelName">The name of the model.</param>
    /// <returns>The <see cref="Encoder"/> for the <paramref name="modelName"/> provided.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="modelName"/> is not found and no
    /// model starts with the <paramref name="modelName"/>.</exception>
    public static async Task<Encoder> EncodingForModelAsync(string modelName)
    {
        // Returns the encoding used by a model.
        string? encodingName = null;
        if (ModelToEncoding.ContainsKey(modelName))
            encodingName = ModelToEncoding[modelName];
        else
        {
            // Check if the model matches a known prefix
            // Prefix matching avoids needing library updates for every model version release
            // Note that this can match on non-existent models (e.g., gpt-3.5-turbo-FAKE)
            foreach (var key in _modelSearchOrder)
                if (modelName.StartsWith(key))
                    return await GetEncodingAsync(ModelToEncoding[key]);
        }

        if (encodingName == null)
            throw new KeyNotFoundException
                ( $"Could not automatically map {modelName} to a tokenizer. " 
                + $"Please use `{nameof(EncodingRegistry)}.{nameof(GetEncodingAsync)}` to explicitly get the tokenizer you expect." );

        return await GetEncodingAsync(encodingName);
    }
}