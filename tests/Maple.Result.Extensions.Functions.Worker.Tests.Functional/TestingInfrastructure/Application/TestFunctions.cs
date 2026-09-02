using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;

/// <summary>
///     Declares a separate HTTP trigger body for every scenario covered by the functional tests.
/// </summary>
internal static class TestFunctions
{
    #region success

    internal static HttpResponseData Success(HttpRequestData request)
    {
        return Result.Success().ToHttpResponseData(request);
    }

    internal static HttpResponseData SuccessValue(HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(13, "Test value")).ToHttpResponseData(request);
    }

    internal static HttpResponseData SuccessNullValue(HttpRequestData request)
    {
        return Result<TestValue?>.FromValue(null).ToHttpResponseData(request);
    }

    #endregion

    #region errors

    internal static HttpResponseData FailureError(HttpRequestData request)
    {
        return Result.FromError(CreateFailureError()).ToHttpResponseData(request);
    }

    internal static HttpResponseData ErrorNotFound(HttpRequestData request)
    {
        var error = Error.NotFound(
            ErrorUri.Tag("tag:test.com,2026:not-found"),
            "Not found title",
            "Not found detail.",
            ErrorUri.Locator("https://test.com/instances/not-found"));

        return Result.FromError(error).ToHttpResponseData(request);
    }

    internal static HttpResponseData ErrorWithDetails(HttpRequestData request)
    {
        var error = Error.Validation(
                ErrorUri.Tag("tag:test.com,2026:validation"),
                "Validation title",
                "Validation detail.",
                ErrorUri.Locator("https://test.com/instances/validation"),
                "errors.validation.detail",
                ("errorCode", "V17"))
            .AddDetail("#/property1", "Property 1 failure detail.", "errors.failure.property1", ("pk1", "pv1"))
            .AddDetail("#/property2", "Property 2 failure detail.");

        return Result.FromError(error).ToHttpResponseData(request);
    }

    internal static HttpResponseData ErrorCustomMapping(HttpRequestData request)
    {
        return Result.FromError(CreateFailureError()).ToHttpResponseData(request, MapFailureToPaymentRequired);
    }

    internal static HttpResponseData ErrorCustomMappingNotMatching(HttpRequestData request)
    {
        return Result.FromError(CreateFailureError()).ToHttpResponseData(request, MapConflictToPaymentRequired);
    }

    #endregion

    #region success status codes

    internal static HttpResponseData StatusCodeSuccess(HttpRequestData request)
    {
        return Result.Success().ToHttpResponseData(request, HttpStatusCode.Accepted);
    }

    internal static HttpResponseData StatusCodeSuccessValue(HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(13, "Test value"))
            .ToHttpResponseData(request, HttpStatusCode.Created);
    }

    internal static HttpResponseData StatusCodeSuccessNullValue(HttpRequestData request)
    {
        return Result<TestValue?>.FromValue(null).ToHttpResponseData(request, HttpStatusCode.IMUsed);
    }

    internal static HttpResponseData StatusCodeSuccessNoResponseStatusCode(HttpRequestData request)
    {
        return Result<TestValue?>.FromValue(null)
            .ToHttpResponseData(request, HttpStatusCode.NonAuthoritativeInformation, HttpStatusCode.ResetContent);
    }

    internal static HttpResponseData StatusCodeError(HttpRequestData request)
    {
        return Result.FromError(CreateFailureError()).ToHttpResponseData(request, HttpStatusCode.Accepted);
    }

    #endregion

    #region success mappings

    internal static HttpResponseData SuccessMapping(HttpRequestData request)
    {
        return Result.Success().ToHttpResponseData(request, MapSuccess);
    }

    internal static HttpResponseData SuccessValueMapping(HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(13, "Test value"))
            .ToHttpResponseData(request, MapValue);
    }

    #endregion

    #region registered custom mappings

    /// <summary>
    ///     Returns an error of the given category, so that a mapping registered with
    ///     the <see cref="ServiceCollectionExtensions.ConfigureResultMapping" /> method can match it.
    /// </summary>
    internal static HttpResponseData CategoryError(HttpRequestData request, ErrorCategory category)
    {
        return Result.FromError(CreateError(category)).ToHttpResponseData(request);
    }

    /// <summary>
    ///     Returns an error matched by both a per-call mapping and a registered mapping, so that
    ///     their precedence can be asserted.
    /// </summary>
    internal static HttpResponseData CategoryErrorWithPerCallMapping(HttpRequestData request, ErrorCategory category)
    {
        return Result.FromError(CreateError(category)).ToHttpResponseData(request, MapAnyToTeapot);
    }

    #endregion

    #region helper methods

    private static Error CreateError(ErrorCategory category)
    {
        var typeUri = ErrorUri.Tag($"tag:test.com,2026:{category}".ToLowerInvariant());
        var title = $"{category} title";

        return category switch
        {
            ErrorCategory.Validation => Error.Validation(typeUri, title),
            ErrorCategory.Unauthenticated => Error.Unauthenticated(typeUri, title),
            ErrorCategory.Unauthorized => Error.Unauthorized(typeUri, title),
            ErrorCategory.NotFound => Error.NotFound(typeUri, title),
            ErrorCategory.Timeout => Error.Timeout(typeUri, title),
            ErrorCategory.Conflict => Error.Conflict(typeUri, title),
            ErrorCategory.Failure => Error.Failure(typeUri, title),
            ErrorCategory.Critical => Error.Critical(typeUri, title),
            ErrorCategory.NotImplemented => Error.NotImplemented(typeUri, title),
            ErrorCategory.Unavailable => Error.Unavailable(typeUri, title),
            _ => throw new NotSupportedException($"Unsupported ErrorCategory: {category}")
        };
    }

    private static HttpResponseData? MapAnyToTeapot(Error error, HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(18, error.Title))
            .ToHttpResponseData(request, (HttpStatusCode)418);
    }

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag("tag:test.com,2026:failure"),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator("https://test.com/instances/failure"));
    }

    private static HttpResponseData MapSuccess(HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(31, "Mapped success"))
            .ToHttpResponseData(request, HttpStatusCode.Accepted);
    }

    private static HttpResponseData MapValue(TestValue value, HttpRequestData request)
    {
        return Result<TestValue>.FromValue(new TestValue(26, value.Name))
            .ToHttpResponseData(request, HttpStatusCode.Created);
    }

    private static HttpResponseData? MapFailureToPaymentRequired(Error error, HttpRequestData request)
    {
        return error.Category == ErrorCategory.Failure
            ? Result<TestValue>.FromValue(new TestValue(11, error.Title))
                .ToHttpResponseData(request, HttpStatusCode.PaymentRequired)
            : null;
    }

    private static HttpResponseData? MapConflictToPaymentRequired(Error error, HttpRequestData request)
    {
        return error.Category == ErrorCategory.Conflict
            ? request.CreateResponse(HttpStatusCode.PaymentRequired)
            : null;
    }

    #endregion
}
