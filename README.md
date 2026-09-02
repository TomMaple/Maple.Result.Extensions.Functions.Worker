# Maple.Result.Extensions.Functions.Worker
Maps the [Maple.Result](https://github.com/TomMaple/Result) to the HTTP response (Azure Functions isolated worker model version)

# Project status: EARLY STAGE

Check also my libraries:
* [Maple.Result](https://github.com/TomMaple/Result),
* [Maple.Result.Extensions.AspNetCore](https://github.com/TomMaple/Maple.Result.Extensions.AspNetCore),
* [Maple.Result.Extensions.MinimalApi](https://github.com/TomMaple/Maple.Result.Extensions.MinimalApi),
* [Maple.Result.Extensions.HttpClient](https://github.com/TomMaple/Maple.Result.Extensions.HttpClient).

# Give it a star ⭐
Do you like it? Show your support by giving this project a star!

# Status
✅ Basic mapping without configuration.  
✅ Unit and integration tests.  
✅ Custom error mapping passed as a parameter to the extension method.  
✅ Custom error mapping registered in the service collection.  
✅ Support for custom successful status codes.  
🔲 Documentation.

# Scope
This package targets the Azure Functions **isolated worker model with the ASP.NET Core integration disabled**, where an HTTP trigger takes an `HttpRequestData` and returns an `HttpResponseData`. When the ASP.NET Core integration is enabled, the trigger works with `HttpRequest`/`IActionResult` instead, and [Maple.Result.Extensions.AspNetCore](https://github.com/TomMaple/Maple.Result.Extensions.AspNetCore) or [Maple.Result.Extensions.MinimalApi](https://github.com/TomMaple/Maple.Result.Extensions.MinimalApi) applies.

The worker model references neither ASP.NET Core nor its `ProblemDetails` type, so this package writes the RFC 9457 body itself, with the `application/problem+json` content type.

# Usage
```csharp
using Maple.Result.Extensions.Functions.Worker;

// Success -> 204 No Content, error -> RFC 9457 problem details
[Function("DeleteOrder")]
public HttpResponseData Delete([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "orders/{id}")] HttpRequestData request, int id)
{
    return orderService.Delete(id).ToHttpResponseData(request);
}

// Success -> 200 OK with the value (204 No Content when the value is null)
[Function("GetOrder")]
public HttpResponseData Get([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{id}")] HttpRequestData request, int id)
{
    return orderService.Get(id).ToHttpResponseData(request);
}

// Success -> the given status code
[Function("CreateOrder")]
public HttpResponseData Create([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData request, Order order)
{
    return orderService.Create(order).ToHttpResponseData(request, HttpStatusCode.Created);
}

// Success -> mapped explicitly, e.g. to return the Location header
[Function("CreateOrderWithLocation")]
public HttpResponseData CreateWithLocation([HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData request, Order order)
{
    return orderService.Create(order).ToHttpResponseData(request, (Order created, HttpRequestData req) =>
    {
        var response = Result<Order>.FromValue(created).ToHttpResponseData(req, HttpStatusCode.Created);
        response.Headers.Add("Location", $"orders/{created.Id}");

        return response;
    });
}

// Errors -> mapped explicitly; the default mapping is used when the mapping returns null
[Function("ListOrders")]
public HttpResponseData List([HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders")] HttpRequestData request)
{
    return orderService.GetAll().ToHttpResponseData(request,
        (error, req) => error.Category == ErrorCategory.Conflict ? req.CreateResponse(HttpStatusCode.Gone) : null);
}
```

Custom error mappings can also be registered once for the whole worker. They are evaluated after the mapping
passed to the extension method, and in the order they were added:
```csharp
var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.ConfigureResultMapping(options =>
        options.ErrorMappings.Add((error, request) => error.Category == ErrorCategory.Conflict
            ? request.CreateResponse(HttpStatusCode.Gone)
            : null));

await builder.Build().RunAsync();
```

# Example Error Response
Example
```json
{
    "type": "https://example.com/probs/out-of-credit",
    "title": "You do not have enough credit.",
    "status": 400,
    "detail": "Your current balance is 30, but that costs 50.",
    "instance": "/accounts/12345/msgs/abc",
    "errors": [
        {
            "pointer": "#/age",
            "detail": "must be a positive integer",
            "detailTemplated": {
                "templateId": "user.details.age.mustBePositive"
            }
        },
        {
            "pointer": "#/profile/colour",
            "detail": "must be ‘green’, ‘red’ or ‘blue’",
            "detailTemplated": {
                "templateId": "user.profile.colour",
                "params": {
                    "validValueIds": [
                        "user.profile.colour.green",
                        "user.profile.colour.red",
                        "user.profile.colour.blue"
                    ]
                }
            }
        }
    ],
    "detailTemplated": {
        "templateId": "user.account.balance.tooLow",
        "params": {
            "errorCode": "UAB17",
            "accounts": [
                {
                    "title": "Main (***9456)",
                    "url": "/accounts/12345"
                },
                {
                    "title": "Main (***3357)",
                    "url": "/accounts/67890"
                }
            ],
            "currentBalance": 30,
            "requiredBalance": 50
        }
    }
}
```
Unlike the ASP.NET Core versions, the response carries no `traceId` extension: the isolated worker model has
no equivalent of the ASP.NET Core *Problem Details* service adding it.

## See also
* [Problem Details for HTTP APIs - RFC 7807 is dead, long live RFC 9457](https://blog.frankel.ch/problem-details-http-apis/)
* [tag URI scheme](https://en.wikipedia.org/wiki/Tag_URI_scheme)
* [RFC 1738: Uniform Resource Locators (URL)](https://datatracker.ietf.org/doc/html/rfc1738)
* [RFC 3986: Uniform Resource Identifier (URI): Generic Syntax](https://datatracker.ietf.org/doc/html/rfc3986)
* [RFC 4151: The 'tag' URI Scheme](https://datatracker.ietf.org/doc/html/rfc4151)
* [RFC 6901: JavaScript Object Notation (JSON) Pointer](https://datatracker.ietf.org/doc/html/rfc6901)
* [RFC 9457: Problem Details for HTTP APIs](https://datatracker.ietf.org/doc/html/rfc9457)

# Contribution
Please contact author: engineer(at sign)blumail(dot)me
