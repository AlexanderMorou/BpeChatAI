using static BpeTokenizer.BytePairEncodingRegistry;
// Converted from tiktoken /model.py
// See https://github.com/openai/tiktoken/blob/main/tiktoken/model.py

namespace BpeTokenizer;

/// <summary>This class contains the model names and their corresponding encodings.</summary>
public class BytePairEncodingModels
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
          // text
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
    /// <summary>Gets the encoding for the <paramref name="modelName"/> provided.</summary>
    /// <param name="modelName">The name of the model.</param>
    /// <returns>The <see cref="BytePairEncoder"/> for the <paramref name="modelName"/> provided.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the <paramref name="modelName"/> is not found and no
    /// model starts with the <paramref name="modelName"/>.</exception>
    public static async Task<BytePairEncoder> EncodingForModelAsync(string modelName)
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
            foreach (var item in ModelPrefixToEncoding)
                if (modelName.StartsWith(item.Key))
                    return await GetEncodingAsync(item.Value);
        }

        if (encodingName == null)
            throw new KeyNotFoundException
                ( $"Could not automatically map {modelName} to a tokenizer. " 
                + $"Please use `{nameof(BytePairEncodingRegistry)}.{nameof(GetEncodingAsync)}` to explicitly get the tokenizer you expect." );

        return await GetEncodingAsync(encodingName);
    }
}