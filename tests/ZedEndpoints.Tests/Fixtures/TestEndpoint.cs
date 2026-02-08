using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Test endpoint that registers a simple GET route.
/// </summary>
public sealed class TestEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/test", () => Results.Ok("Test endpoint"))
            .WithName("TestEndpoint");
    }
}