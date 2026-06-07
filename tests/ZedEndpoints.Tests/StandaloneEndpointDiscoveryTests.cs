using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Extensions;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for automatic discovery of IEndpoint implementations not belonging to any IEndpointGroup.
/// </summary>
public sealed class StandaloneEndpointDiscoveryTests
{
    [Fact]
    public async Task MapEndpointGroups_Should_Discover_Standalone_IEndpoint_Implementations()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(assembly: Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();
            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "StandaloneEndpoint"),
                "StandaloneEndpoint implements IEndpoint without belonging to any IEndpointGroup and should be auto-discovered");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_Should_Not_Double_Register_Endpoints_Already_In_A_Group()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(assembly: Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var testEndpoints = app.GetEndpoints()
                .Where(e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"))
                .ToList();

            testEndpoints.Should().HaveCount(1,
                "TestEndpoint is already registered through TestEndpointGroup and must not be registered again as a standalone endpoint");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Not_Double_Register_Endpoints_Already_In_A_Group()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups("api/v1", Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var testEndpoints = app.GetEndpoints()
                .Where(e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"))
                .ToList();

            testEndpoints.Should().HaveCount(1,
                "TestEndpoint is already registered through TestEndpointGroup and must not be registered again as a standalone endpoint even with a global prefix");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Apply_Prefix_To_Standalone_Endpoints()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups("api/v1", Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var standaloneEndpoint = app.GetEndpoints()
                .OfType<RouteEndpoint>()
                .Should().ContainSingle(
                    e => e.Metadata.OfType<IEndpointNameMetadata>()
                        .Any(m => m.EndpointName == "StandaloneEndpoint"),
                    "StandaloneEndpoint should be registered")
                .Which;

            standaloneEndpoint.RoutePattern.RawText.Should().StartWith("api/v1",
                "standalone endpoints without [NoGlobalPrefix] should receive the global prefix");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Not_Prefix_Standalone_Endpoints_With_NoGlobalPrefixAttribute()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups("api/v1", Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var standaloneEndpoint = app.GetEndpoints()
                .OfType<RouteEndpoint>()
                .Should().ContainSingle(
                    e => e.Metadata.OfType<IEndpointNameMetadata>()
                        .Any(m => m.EndpointName == "StandaloneNoGlobalPrefixEndpoint"),
                    "StandaloneNoGlobalPrefixEndpoint should be registered")
                .Which;

            standaloneEndpoint.RoutePattern.RawText.Should().NotStartWith("api/v1",
                "standalone endpoints with [NoGlobalPrefix] should ignore the global prefix");

            standaloneEndpoint.RoutePattern.RawText.Should().Be("/standalone-nopfx",
                "route should remain as originally defined");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithNullPrefix_Should_Discover_Standalone_At_Original_Route()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(assembly: Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var standaloneEndpoint = app.GetEndpoints()
                .OfType<RouteEndpoint>()
                .Should().ContainSingle(
                    e => e.Metadata.OfType<IEndpointNameMetadata>()
                        .Any(m => m.EndpointName == "StandaloneEndpoint"),
                    "StandaloneEndpoint should be registered")
                .Which;

            standaloneEndpoint.RoutePattern.RawText.Should().Be("/standalone",
                "without a global prefix the route should be exactly as defined");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }
}
