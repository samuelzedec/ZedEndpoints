using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Another test endpoint for testing multiple endpoint discovery.
/// </summary>
public sealed class AnotherTestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/another", () => Results.Created("/another/1", new { Id = 1 }))
            .WithName("AnotherTestEndpoint");
    }
}