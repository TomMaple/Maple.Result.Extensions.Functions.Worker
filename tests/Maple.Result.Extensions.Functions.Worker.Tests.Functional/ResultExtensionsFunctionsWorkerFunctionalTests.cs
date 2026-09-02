using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Fixtures;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Helpers;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Http;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional;

public class ResultExtensionsFunctionsWorkerFunctionalTests
    : IClassFixture<TestWorkerFixture>, IClassFixture<CustomMappingWorkerFixture>
{
    #region consts

    private const string JsonContentType = "application/json; charset=utf-8";
    private const string ProblemJsonContentType = "application/problem+json; charset=utf-8";

    private const string ExpectedValueJson =
        """
        {
          "id": 13,
          "name": "Test value"
        }
        """;

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

    private const string ExpectedNotFoundJson =
        """
        {
          "type": "tag:test.com,2026:not-found",
          "title": "Not found title",
          "status": 404,
          "detail": "Not found detail.",
          "instance": "https://test.com/instances/not-found"
        }
        """;

    private const string ExpectedValidationJson =
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
          ],
          "detailTemplated": {
            "templateId": "errors.validation.detail",
            "params": {
              "errorCode": "V17"
            }
          }
        }
        """;

    private const string ExpectedFailureMappingJson =
        """
        {
          "id": 11,
          "name": "Failure title"
        }
        """;

    private const string ExpectedMappedSuccessJson =
        """
        {
          "id": 31,
          "name": "Mapped success"
        }
        """;

    private const string ExpectedMappedValueJson =
        """
        {
          "id": 26,
          "name": "Test value"
        }
        """;

    #endregion

    #region read-only fields

    private readonly TestWorkerFixture _fixture;
    private readonly CustomMappingWorkerFixture _fixtureWithCustomMapping;

    #endregion

    #region constructors

    public ResultExtensionsFunctionsWorkerFunctionalTests(TestWorkerFixture fixture,
        CustomMappingWorkerFixture customMappingFixture)
    {
        _fixture = fixture;
        _fixtureWithCustomMapping = customMappingFixture;
    }

    #endregion

    #region success

    [Fact]
    public void ToHttpResponseData_SuccessfulResult_ReturnsNoContent()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.Success(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
        response.ReadContentType().ShouldBeNull();
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValue_ReturnsOkWithValue()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.SuccessValue(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.ReadContentType().ShouldBe(JsonContentType);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValue_ReturnsNoContent()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.SuccessNullValue(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    #endregion

    #region errors

    [Fact]
    public void ToHttpResponseData_FailureError_ReturnsUnprocessableEntityProblemDetails()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.FailureError(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ReadContentType().ShouldBe(ProblemJsonContentType);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    [Fact]
    public void ToHttpResponseData_NotFoundError_ReturnsNotFoundProblemDetails()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.ErrorNotFound(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedNotFoundJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithDetailsAndTemplatedDetail_ReturnsBothExtensions()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.ErrorWithDetails(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValidationJson));
    }

    #endregion

    #region success status codes

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithStatusCode_ReturnsTheGivenStatusCode()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.StatusCodeSuccess(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndStatusCode_ReturnsTheGivenStatusCodeWithValue()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.StatusCodeSuccessValue(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedValueJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValueAndStatusCode_ReturnsTheSameStatusCode()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.StatusCodeSuccessNullValue(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.IMUsed);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithNullValueAndNoResponseStatusCode_ReturnsTheNoResponseStatusCode()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.StatusCodeSuccessNoResponseStatusCode(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.ResetContent);
        response.ReadBodyAsJson().ShouldBeEmpty();
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithStatusCode_IgnoresTheSuccessStatusCode()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.StatusCodeError(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region success mappings

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithSuccessMapping_UsesTheMapping()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.SuccessMapping(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedMappedSuccessJson));
    }

    [Fact]
    public void ToHttpResponseData_SuccessfulResultWithValueAndSuccessMapping_UsesTheMapping()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.SuccessValueMapping(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedMappedValueJson));
    }

    #endregion

    #region per-call custom error mappings

    [Fact]
    public void ToHttpResponseData_ErrorWithMatchingPerCallMapping_UsesTheMapping()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.ErrorCustomMapping(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.PaymentRequired);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureMappingJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorWithNotMatchingPerCallMapping_FallsBackToTheDefaultMapping()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.ErrorCustomMappingNotMatching(request);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedFailureJson));
    }

    #endregion

    #region registered custom error mappings

    [Fact]
    public void ToHttpResponseData_ErrorMatchedByRegisteredMappingOnCategory_UsesThatMapping()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 99,
              "name": "Conflict title"
            }
            """;

        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.Conflict);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Gone);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorMatchedByRegisteredMappingOnTypeUri_UsesThatMapping()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 42,
              "name": "tag:test.com,2026:validation"
            }
            """;

        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.Validation);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorMatchedByTwoRegisteredMappings_UsesTheFirstOne()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 1,
              "name": "First matching mapping"
            }
            """;

        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.Timeout);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.UpgradeRequired);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_RegisteredMappingUsingTheRequest_CanShapeTheWholeResponse()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 7,
              "name": "NotImplemented title"
            }
            """;

        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.NotImplemented);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ReadHeader(response, CustomMappingWorkerFixture.ErrorTitleHeaderName).ShouldBe("NotImplemented title");
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_ErrorNotMatchedByAnyRegisteredMapping_FallsBackToTheDefaultMapping()
    {
        // Arrange
        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.NotFound);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.ReadContentType().ShouldBe(ProblemJsonContentType);
    }

    [Fact]
    public void ToHttpResponseData_ErrorMatchedByBothMappings_PrefersThePerCallMapping()
    {
        // Arrange
        const string ExpectedJson =
            """
            {
              "id": 18,
              "name": "Conflict title"
            }
            """;

        var request = _fixtureWithCustomMapping.CreateRequest();

        // Act
        var response = TestFunctions.CategoryErrorWithPerCallMapping(request, ErrorCategory.Conflict);

        // Assert
        response.StatusCode.ShouldBe((HttpStatusCode)418);
        response.ReadBodyAsJson().ShouldBe(JsonHelper.Normalize(ExpectedJson));
    }

    [Fact]
    public void ToHttpResponseData_RegisteredMappings_AreNotAppliedWhenTheyAreNotRegistered()
    {
        // Arrange
        var request = _fixture.CreateRequest();

        // Act
        var response = TestFunctions.CategoryError(request, ErrorCategory.Conflict);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        response.ReadContentType().ShouldBe(ProblemJsonContentType);
    }

    #endregion

    #region helper methods

    private static string? ReadHeader(HttpResponseData response, string name)
    {
        return response.Headers.TryGetValues(name, out var values)
            ? string.Join(", ", values)
            : null;
    }

    #endregion
}
