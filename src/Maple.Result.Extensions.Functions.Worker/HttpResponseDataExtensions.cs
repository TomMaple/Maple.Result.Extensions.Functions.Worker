using Maple.Result.Extensions.Functions.Worker.Configuration;
using Maple.Result.Extensions.Functions.Worker.Mappers;
using Maple.Result.Extensions.Functions.Worker.Serialization;
using Maple.Result.Extensions.Functions.Worker.ViewModels;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker;

/// <summary>
///     The collection of extension methods for converting <see cref="Result" /> and <see cref="Result{T}" />
///     to the Azure Functions isolated worker <see cref="HttpResponseData" />.
/// </summary>
public static class HttpResponseDataExtensions
{
    #region (Result)

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result" /> instance.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result" />.</returns>
    public static HttpResponseData ToHttpResponseData(this Result result, HttpRequestData request,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);

        return result.Match(
            () => ResponseFactory.CreateEmptyResponse(request, HttpStatusCode.NoContent),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="successStatusCode">The HTTP status code returned when the <see cref="Result" /> is successful.</param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result" />.</returns>
    public static HttpResponseData ToHttpResponseData(this Result result, HttpRequestData request,
        HttpStatusCode successStatusCode, Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);

        return result.Match(
            () => ResponseFactory.CreateEmptyResponse(request, successStatusCode),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result" /> instance, using the given
    ///     mapping function when it is successful.
    /// </summary>
    /// <param name="result">The <see cref="Result" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="customSuccessMapping">
    ///     The mapping function used to convert a successful <see cref="Result" /> to an <see cref="HttpResponseData" />.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result" />.</returns>
    public static HttpResponseData ToHttpResponseData(this Result result, HttpRequestData request,
        Func<HttpRequestData, HttpResponseData> customSuccessMapping,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(customSuccessMapping);

        return result.Match(
            () => customSuccessMapping(request),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    #endregion

    #region (Result<T>)

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result{T}" /> instance.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result{T}" />.</returns>
    public static HttpResponseData ToHttpResponseData<T>(this Result<T> result, HttpRequestData request,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);

        return result.Match(
            value => value is null
                ? ResponseFactory.CreateEmptyResponse(request, HttpStatusCode.NoContent)
                : ResponseFactory.CreateJsonResponse(request, HttpStatusCode.OK, value),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result{T}" /> instance, using the given
    ///     HTTP status code when it is successful.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="successStatusCode">
    ///     The HTTP status code returned when the <see cref="Result{T}" /> is successful. It is also used when
    ///     the <see cref="Result{T}" /> is successful, but its value is <see langword="null" />; use the overload
    ///     taking a <c>successNoResponseStatusCode</c> to return a different status code in that case.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result{T}" />.</returns>
    public static HttpResponseData ToHttpResponseData<T>(this Result<T> result, HttpRequestData request,
        HttpStatusCode successStatusCode, Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        return result.ToHttpResponseData(request, successStatusCode, successStatusCode, customErrorMapping);
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result{T}" /> instance, using the given
    ///     HTTP status codes when it is successful.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="successStatusCode">
    ///     The HTTP status code returned when the <see cref="Result{T}" /> is successful and carries a value.
    /// </param>
    /// <param name="successNoResponseStatusCode">
    ///     The HTTP status code returned when the <see cref="Result{T}" /> is successful, but its value
    ///     is <see langword="null" />.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result{T}" />.</returns>
    public static HttpResponseData ToHttpResponseData<T>(this Result<T> result, HttpRequestData request,
        HttpStatusCode successStatusCode, HttpStatusCode successNoResponseStatusCode,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);

        return result.Match(
            value => value is null
                ? ResponseFactory.CreateEmptyResponse(request, successNoResponseStatusCode)
                : ResponseFactory.CreateJsonResponse(request, successStatusCode, value),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    /// <summary>
    ///     Creates an <see cref="HttpResponseData" /> from a <see cref="Result{T}" /> instance, using the given
    ///     mapping function when it is successful.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the <see cref="Result{T}" /> value used to determine whether to execute
    ///     the passed function. If successful, this is also the type of the parameter passed to that function.
    /// </typeparam>
    /// <param name="result">The <see cref="Result{T}" /> to convert.</param>
    /// <param name="request">The request the response is created for.</param>
    /// <param name="customSuccessMapping">
    ///     The mapping function used to convert the value of a successful <see cref="Result{T}" /> to
    ///     an <see cref="HttpResponseData" />. It is also invoked when the value is <see langword="null" />.
    /// </param>
    /// <param name="customErrorMapping">
    ///     An optional custom mapping function used to convert an <see cref="Error" /> to
    ///     an <see cref="HttpResponseData" />. It is evaluated before the mappings registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method, and it is used
    ///     when it returns a non-<see langword="null" /> value.
    /// </param>
    /// <returns>An <see cref="HttpResponseData" /> representing the <see cref="Result{T}" />.</returns>
    public static HttpResponseData ToHttpResponseData<T>(this Result<T> result, HttpRequestData request,
        Func<T, HttpRequestData, HttpResponseData> customSuccessMapping,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping = null)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(customSuccessMapping);

        return result.Match(
            value => customSuccessMapping(value, request),
            error => error.ToHttpResponseData(request, customErrorMapping));
    }

    #endregion

    #region helper methods

    private static HttpResponseData ToHttpResponseData(this Error error, HttpRequestData request,
        Func<Error, HttpRequestData, HttpResponseData?>? customErrorMapping)
    {
        // Check for the custom mapping passed to the method first
        var mappingResult = customErrorMapping?.Invoke(error, request);
        if (mappingResult is not null)
            return mappingResult;

        // Then check for the registered custom mappings
        mappingResult = TryMapUsingResultMappingOptions(error, request);
        if (mappingResult is not null)
            return mappingResult;

        // Fallback to default mapping
        var statusCode = ErrorCategoryMapper.GetStatusCode(error.Category);
        var problemDetails = new ProblemDetails
        {
            Type = error.TypeUri,
            Title = error.Title,
            Status = (int)statusCode,
            Detail = error.Detail,
            Instance = error.InstanceUri,
            Extensions = ErrorMapper.MapExtensions(error)
        };

        return ResponseFactory.CreateProblemResponse(request, statusCode, problemDetails);
    }

    private static HttpResponseData? TryMapUsingResultMappingOptions(Error error, HttpRequestData request)
    {
        var options = request.FunctionContext.InstanceServices?.GetService<IOptions<ResultMappingOptions>>();
        var mappings = options?.Value.ErrorMappings;
        if (mappings is not { Count: > 0 })
            return null;

        foreach (var mapping in mappings)
        {
            var mappingResult = mapping?.Invoke(error, request);
            if (mappingResult is not null)
                return mappingResult;
        }

        return null;
    }

    #endregion
}
