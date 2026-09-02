using System;
using System.Net;

namespace Maple.Result.Extensions.Functions.Worker.Mappers;

internal static class ErrorCategoryMapper
{
    internal static HttpStatusCode GetStatusCode(ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.Validation => HttpStatusCode.BadRequest,
            ErrorCategory.Unauthenticated => HttpStatusCode.Unauthorized,
            ErrorCategory.Unauthorized => HttpStatusCode.Forbidden,
            ErrorCategory.NotFound => HttpStatusCode.NotFound,
            ErrorCategory.Timeout => HttpStatusCode.RequestTimeout,
            ErrorCategory.Conflict => HttpStatusCode.Conflict,
            ErrorCategory.Failure => HttpStatusCode.UnprocessableEntity,
            ErrorCategory.Critical => HttpStatusCode.InternalServerError,
            ErrorCategory.NotImplemented => HttpStatusCode.NotImplemented,
            ErrorCategory.Unavailable => HttpStatusCode.ServiceUnavailable,
            _ => throw new NotImplementedException($"Unsupported ErrorCategory: {category}")
        };
    }
}
