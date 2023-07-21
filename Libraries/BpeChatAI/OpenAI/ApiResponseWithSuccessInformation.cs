using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BpeChatAI.OpenAI;

/// <summary>Represents a response from the OpenAI API.</summary>
public interface IApiResponseWithSuccessInformation
{
    /// <summary>Gets or sets the error message if there was an error in the response.</summary>
    string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    Exception? Exception { get; set; }
    /// <summary>Gets or sets whether the response was successful or not.</summary>
    bool IsSuccess { get; set; }
}

/// <summary>Represents a response from the OpenAI API.</summary>
public class ApiResponseWithSuccessInformation
    : IApiResponseWithSuccessInformation
{
    /// <summary>Gets or sets whether the response was successful or not.</summary>
    [JsonIgnore()]
    public virtual bool IsSuccess { get; set; }

    /// <summary>Gets or sets the error message if there was an error in the response.</summary>
    [JsonIgnore()]
    public string? ErrorMessage { get; set; }
    /// <summary>Gets or sets the exception if there was a failure making the request.</summary>
    [JsonIgnore()]
    public Exception? Exception { get; set; }
}
