using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;

namespace ZedEndpoints.Tests.Fixtures;

/// <summary>
/// Standalone endpoint that is not registered through any IEndpointGroup.
/// Used to test automatic standalone endpoint discovery.
/// </summary>
public sealed class StandaloneEndpoint : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("/standalone", () => Results.Ok("Standalone endpoint"))
            .WithName("StandaloneEndpoint");
    }
}
