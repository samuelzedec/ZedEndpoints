using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace ZedEndpoints.Tests;

/// <summary>
/// Helper methods for testing endpoints.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Gets the registered endpoints from a WebApplication, forcing their construction.
    /// </summary>
    public static IReadOnlyList<Endpoint> GetEndpoints(this WebApplication app)
    {
        var dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        return dataSource.Endpoints;
    }

    /// <summary>
    /// Asynchronously stops and disposes the resources of a <see cref="WebApplication"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> instance to be stopped and disposed.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task DisposeTestsAsync(this WebApplication app)
    {
        await app.StopAsync();
        await app.DisposeAsync();
    }

    /// <summary>
    /// Creates and configures a <see cref="WebApplicationBuilder"/> for testing purposes.
    /// </summary>
    /// <returns>A configured instance of <see cref="WebApplicationBuilder"/>.</returns>
    public static WebApplicationBuilder CreateTestBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        return builder;
    }
}