using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Attributes;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Standalone endpoint with [NoGlobalPrefix] that is not part of any IEndpointGroup.
/// Used to test that the attribute works correctly for standalone endpoint discovery.
/// </summary>
[NoGlobalPrefix]
public sealed class StandaloneNoGlobalPrefixEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/standalone-nopfx", () => Results.Ok())
            .WithName("StandaloneNoGlobalPrefixEndpoint");
    }
}
