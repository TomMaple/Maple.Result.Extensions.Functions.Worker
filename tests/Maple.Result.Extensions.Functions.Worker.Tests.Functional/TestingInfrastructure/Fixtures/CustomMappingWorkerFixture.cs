using Maple.Result.Extensions.Functions.Worker.Configuration;
using Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application.Models;
using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Fixtures;

public sealed class CustomMappingWorkerFixture : TestWorkerFixture
{
    #region consts

    internal const string ErrorTitleHeaderName = "x-error-title";

    private const string ValidationTypeUri = "tag:test.com,2026:validation";

    #endregion

    private protected override Action<ResultMappingOptions>? ConfigureResultMapping => Configure;

    #region helper methods

    private static void Configure(ResultMappingOptions options)
    {
        // Matches the errors of the ErrorCategory.Conflict category.
        options.ErrorMappings.Add((error, request) => error.Category == ErrorCategory.Conflict
            ? Result<TestValue>
                .FromValue(new TestValue(99, error.Title))
                .ToHttpResponseData(request, HttpStatusCode.Gone)
            : null);

        // Matches a single error type, regardless of its category.
        options.ErrorMappings.Add((error, request) => error.TypeUri == ValidationTypeUri
            ? Result<TestValue>
                .FromValue(new TestValue(42, error.TypeUri))
                .ToHttpResponseData(request, HttpStatusCode.TooManyRequests)
            : null);

        // The first of the two mappings matching the ErrorCategory.Timeout category wins.
        options.ErrorMappings.Add((error, request) => error.Category == ErrorCategory.Timeout
            ? Result<TestValue>
                .FromValue(new TestValue(1, "First matching mapping"))
                .ToHttpResponseData(request, HttpStatusCode.UpgradeRequired)
            : null);
        options.ErrorMappings.Add((error, request) => error.Category == ErrorCategory.Timeout
            ? Result<TestValue>
                .FromValue(new TestValue(2, "Second matching mapping"))
                .ToHttpResponseData(request, HttpStatusCode.FailedDependency)
            : null);

        // Adds a response header on top of returning the value.
        options.ErrorMappings.Add((error, request) =>
        {
            if (error.Category != ErrorCategory.NotImplemented)
                return null;

            var response = Result<TestValue>
                .FromValue(new TestValue(7, error.Title))
                .ToHttpResponseData(request, HttpStatusCode.OK);

            response.Headers.Add(ErrorTitleHeaderName, error.Title);

            return response;
        });
    }

    #endregion
}
