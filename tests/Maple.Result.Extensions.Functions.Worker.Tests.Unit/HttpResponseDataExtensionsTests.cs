using Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Helpers;
using Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Http;
using Maple.Result.Extensions.Functions.Worker.Tests.Unit.TestingInfrastructure.Models;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Unit;

public class HttpResponseDataExtensionsTests
{
    #region consts

    private const string JsonContentType = "application/json; charset=utf-8";
    private const string ProblemJsonContentType = "application/problem+json; charset=utf-8";

    private const string FailureTypeUri = "tag:test.com,2026:failure";
    private const string FailureInstanceUri = "https://test.com/instances/failure";

    private const string ExpectedFailureJson =
        """
        {
          "type": "tag:test.com,2026:failure",
          "title": "Failure title",
          "status": 422,
          "detail": "Failure detail.",
          "instance": "https://test.com/instances/failure"
        }
        """;

    private const string ExpectedValueJson =
        """
        {
          "id": 13,
          "name": "Test value"
        }
        """;

    #endregion

    #region (Result) default mapping

    [Fact]
    public void ToHttpResponseData_SuccessfulResult_ReturnsNoContent()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.Success();

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        var testResponse = response.ShouldBeOfType<TestHttpResponseData>();
        testResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        testResponse.ReadBody().ShouldBeEmpty();
        testResponse.GetContentType().ShouldBeNull();
    }

    [Fact]
    public void ToHttpResponseData_Error_ReturnsProblemDetailsMappedFromTheError()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        var testResponse = response.ShouldBeOfType<TestHttpResponseData>();
        testResponse.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        testResponse.GetContentType().ShouldBe(ProblemJsonContentType);
        testResponse.ReadBody().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Theory]
    [InlineData(ErrorCategory.Validation, HttpStatusCode.BadRequest)]
    [InlineData(ErrorCategory.Unauthenticated, HttpStatusCode.Unauthorized)]
    [InlineData(ErrorCategory.Unauthorized, HttpStatusCode.Forbidden)]
    [InlineData(ErrorCategory.NotFound, HttpStatusCode.NotFound)]
    [InlineData(ErrorCategory.Timeout, HttpStatusCode.RequestTimeout)]
    [InlineData(ErrorCategory.Conflict, HttpStatusCode.Conflict)]
    [InlineData(ErrorCategory.Failure, HttpStatusCode.UnprocessableEntity)]
    [InlineData(ErrorCategory.Critical, HttpStatusCode.InternalServerError)]
    [InlineData(ErrorCategory.NotImplemented, HttpStatusCode.NotImplemented)]
    [InlineData(ErrorCategory.Unavailable, HttpStatusCode.ServiceUnavailable)]
    public void ToHttpResponseData_ErrorOfTheGivenCategory_ReturnsExpectedStatusCode(
        ErrorCategory category, HttpStatusCode expectedStatusCode)
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateError(category));

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        response.StatusCode.ShouldBe(expectedStatusCode);
        response.ReadBodyAsJson().ShouldContain($"\"status\":{(int)expectedStatusCode}");
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithTemplatedDetail_AddsTheDetailTemplatedExtension()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var error = Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri),
            "errors.failure.detail",
            ("key1", "value1"), ("key2", 123));

        var sut = Result.FromError(error);

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:failure",
              "title": "Failure title",
              "status": 422,
              "detail": "Failure detail.",
              "instance": "https://test.com/instances/failure",
              "detailTemplated": {
                "templateId": "errors.failure.detail",
                "params": {
                  "key1": "value1",
                  "key2": 123
                }
              }
            }
            """;

        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithDetails_AddsTheErrorsExtension()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var error = Error.Validation(
                ErrorUri.Tag("tag:test.com,2026:validation"),
                "Validation title",
                "Validation detail.",
                ErrorUri.Locator("https://test.com/instances/validation"))
            .AddDetail("#/property1", "Property 1 failure detail.", "errors.failure.property1", ("pk1", "pv1"))
            .AddDetail("#/property2", "Property 2 failure detail.");

        var sut = Result.FromError(error);

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        const string ExpectedJson =
            """
            {
              "type": "tag:test.com,2026:validation",
              "title": "Validation title",
              "status": 400,
              "detail": "Validation detail.",
              "instance": "https://test.com/instances/validation",
              "errors": [
                {
                  "pointer": "#/property1",
                  "detail": "Property 1 failure detail.",
                  "detailTemplated": {
                    "templateId": "errors.failure.property1",
                    "params": {
                      "pk1": "pv1"
                    }
                  }
                },
                {
                  "pointer": "#/property2",
                  "detail": "Property 2 failure detail."
                }
              ]
            }
            """;

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    #endregion

    #region (Result) success status code

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithStatusCode_ReturnsTheGivenStatusCode()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.Success();

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.Accepted);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithStatusCode_IgnoresTheSuccessStatusCode()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.Accepted);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region (Result) success mapping

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithSuccessMapping_UsesTheMapping()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.Success();

        // Act
        var response = sut.ToHttpResponseData(request, CreateGoneResponse);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithSuccessMapping_IgnoresTheMapping()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request, CreateGoneResponse);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
    }

    #endregion

    #region (Result) custom error mapping

    [Fact]
    public void ToHttpResponseData_ErrorWithMatchingCustomErrorMapping_UsesTheMapping()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request, MapFailureToPaymentRequired);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithNotMatchingCustomErrorMapping_FallsBackToTheDefaultMapping()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request, MapConflictToPaymentRequired);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region (Result<T>) default mapping

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValue_ReturnsOkWithTheValue()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        var testResponse = response.ShouldBeOfType<TestHttpResponseData>();
        testResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        testResponse.GetContentType().ShouldBe(JsonContentType);
        testResponse.ReadBody().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_ErrorOfResultWithValue_ReturnsProblemDetailsMappedFromTheError()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue>.FromError(CreateFailureError());

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region (Result<T>) success status codes

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndStatusCode_ReturnsTheGivenStatusCodeWithTheValue()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.Created);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValueAndStatusCode_ReturnsTheSameStatusCode()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.Created);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValueAndNoResponseStatusCode_ReturnsTheNoResponseStatusCode()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue?>.FromValue(null);

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.NonAuthoritativeInformation,
            HttpStatusCode.ResetContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ResetContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndNoResponseStatusCode_ReturnsTheSuccessStatusCode()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue?>.FromValue(new TestValue(13, "Test value"));

        // Act
        var response = sut.ToHttpResponseData(request, HttpStatusCode.NonAuthoritativeInformation,
            HttpStatusCode.ResetContent);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NonAuthoritativeInformation);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    #endregion

    #region (Result<T>) success mapping

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndSuccessMapping_UsesTheMapping()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act
        var response = sut.ToHttpResponseData(request, MapValueToCreated);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValueAndSuccessMapping_InvokesTheMappingWithTheNullValue()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue?>.FromValue(null);
        var mappingWasInvokedWithNull = false;

        // Act
        // The lambda parameters are typed explicitly, because an implicitly typed lambda is convertible to
        // the customErrorMapping parameter of the other overload as well.
        var response = sut.ToHttpResponseData(request, (TestValue? value, HttpRequestData req) =>
        {
            mappingWasInvokedWithNull = value is null;

            return req.CreateResponse(HttpStatusCode.Gone);
        });

        // Assert
        mappingWasInvokedWithNull.ShouldBeTrue();
        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
    }

    #endregion

    #region argument guards

    [Fact]
    public void ToHttpResponseData_NullResult_ThrowsArgumentNullException()
    {
        // Arrange
        var request = TestHttpRequestData.Create();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => ((Result)null!).ToHttpResponseData(request));
    }

    [Fact]
    public void ToHttpResponseData_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => sut.ToHttpResponseData(null!));
    }

    [Fact]
    public void ToHttpResponseData_NullSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result.Success();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            sut.ToHttpResponseData(request, (Func<HttpRequestData, HttpResponseData>)null!));
    }

    [Fact]
    public void ToHttpResponseData_NullValueSuccessMapping_ThrowsArgumentNullException()
    {
        // Arrange
        var request = TestHttpRequestData.Create();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            sut.ToHttpResponseData(request, (Func<TestValue, HttpRequestData, HttpResponseData>)null!));
    }

    #endregion

    #region worker serializer

    /// <summary>
    ///     The bodies are written with the WriteAsJsonAsync method of the worker SDK, which resolves
    ///     the serializer from the worker services and has no fallback of its own. A worker missing that
    ///     registration therefore fails on the first response carrying a body.
    /// </summary>
    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndNoWorkerSerializer_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = TestHttpRequestData.CreateWithoutWorkerDefaults();
        var sut = Result<TestValue>.FromValue(new TestValue(13, "Test value"));

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.ToHttpResponseData(request));
    }

    [Fact]
    public void ToHttpResponseData_ErrorAndNoWorkerSerializer_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = TestHttpRequestData.CreateWithoutWorkerDefaults();
        var sut = Result.FromError(CreateFailureError());

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => sut.ToHttpResponseData(request));
    }

    /// <summary>
    ///     A response without a body is created without touching the serializer, so it stays usable
    ///     even when the worker has none registered.
    /// </summary>
    [Fact]
    public void ToHttpResponseData_SuccessfulResultAndNoWorkerSerializer_ReturnsNoContent()
    {
        // Arrange
        var request = TestHttpRequestData.CreateWithoutWorkerDefaults();
        var sut = Result.Success();

        // Act
        var response = sut.ToHttpResponseData(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    #endregion

    #region helper methods

    private static Error CreateFailureError()
    {
        return Error.Failure(
            ErrorUri.Tag(FailureTypeUri),
            "Failure title",
            "Failure detail.",
            ErrorUri.Locator(FailureInstanceUri));
    }

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

    private static HttpResponseData CreateGoneResponse(HttpRequestData request)
    {
        return request.CreateResponse(HttpStatusCode.Gone);
    }

    private static HttpResponseData MapValueToCreated(TestValue value, HttpRequestData request)
    {
        return Result<TestValue>.FromValue(value).ToHttpResponseData(request, HttpStatusCode.Created);
    }

    private static HttpResponseData? MapFailureToPaymentRequired(Error error, HttpRequestData request)
    {
        return error.Category == ErrorCategory.Failure
            ? request.CreateResponse(HttpStatusCode.PaymentRequired)
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
