using Maple.Result.Extensions.Functions.Worker.ViewModels;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace Maple.Result.Extensions.Functions.Worker.Serialization;

/// <summary>
///     Creates the <see cref="HttpResponseData" /> instances carrying a JSON body, using the serializer
///     configured for the worker, so that a body written by this library is identical to a body written
///     by the <c>WriteAsJsonAsync</c> method of the worker SDK.
/// </summary>
internal static class ResponseFactory
{
    #region consts

    private static readonly string ProblemJsonContentType = $"{MediaTypeNames.Application.ProblemJson}; charset={Encoding.UTF8.WebName}";

    #endregion

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> with the given status code and no body.
    /// </summary>
    internal static HttpResponseData CreateEmptyResponse(HttpRequestData request, HttpStatusCode statusCode)
    {
        return request.CreateResponse(statusCode);
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> with the given status code and the given value
    ///     serialized as an <c>application/json</c> body.
    /// </summary>
    internal static HttpResponseData CreateJsonResponse<T>(HttpRequestData request, HttpStatusCode statusCode, T value)
    {
        var response = request.CreateResponse(statusCode);
        response.WriteAsJsonAsync(value);

        return response;
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> with the given status code and the given problem details
    ///     serialized as an <c>application/problem+json</c> body.
    /// </summary>
    internal static HttpResponseData CreateProblemResponse(HttpRequestData request, HttpStatusCode statusCode,
        ProblemDetails problemDetails)
    {
        var response = request.CreateResponse(statusCode);
        response.WriteAsJsonAsync(problemDetails, ProblemJsonContentType);

        return response;
    }
}
