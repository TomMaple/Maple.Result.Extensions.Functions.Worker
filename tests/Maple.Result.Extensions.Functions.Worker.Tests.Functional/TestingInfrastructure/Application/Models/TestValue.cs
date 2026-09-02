using System.Text.Json.Serialization;

namespace Maple.Result.Extensions.Functions.Worker.Tests.Functional.TestingInfrastructure.Application.Models;

public sealed record TestValue
{
    public TestValue(int id, string name)
    {
        Id = id;
        Name = name;
    }

    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; }
}
