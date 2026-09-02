using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;

namespace Maple.Result.Extensions.Functions.Worker.Configuration;

/// <summary>
///     Represents the options for configuring result mappings.
/// </summary>
public class ResultMappingOptions
{
    /// <summary>
    ///     Gets the list of custom mappings for mapping errors to <see cref="HttpResponseData" />.
    /// </summary>
    public List<Func<Error, HttpRequestData, HttpResponseData?>> ErrorMappings { get; } = [];
}
