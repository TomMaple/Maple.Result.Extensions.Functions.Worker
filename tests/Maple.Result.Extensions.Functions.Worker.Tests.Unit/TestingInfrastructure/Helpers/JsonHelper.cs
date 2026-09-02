using System;
using System.Text.Json.Nodes;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Helpers;

internal static class JsonHelper
{
    /// <summary>
    ///     Returns the given JSON text normalized, so that the formatting of a JSON literal does not affect
    ///     the comparison with a response body.
    /// </summary>
    internal static string Normalize(string json)
    {
        var node = JsonNode.Parse(json)
                   ?? throw new InvalidOperationException("The JSON text is not a valid JSON document.");

        return node.ToJsonString();
    }
}
