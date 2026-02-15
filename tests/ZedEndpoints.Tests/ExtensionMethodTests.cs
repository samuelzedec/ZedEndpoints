using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Extensions;
using ZedEndpoints.Tests.Fixtures;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for extension method behaviors.
/// </summary>
public sealed class ExtensionMethodTests
{
    [Fact]
    public async Task MapEndpointGroups_Should_Be_Idempotent()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly());
        app.MapEndpointGroups(Assembly.GetExecutingAssembly());

        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();
            var testEndpoints = endpoints
                .Where(e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"))
                .ToList();

            testEndpoints.Should().HaveCount(1,
                "calling MapEndpointGroups multiple times should not duplicate endpoints");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public void MapEndpoint_Should_Invoke_Static_Map_Method()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        var act = () => app.MapEndpoint<TestEndpoint>();

        // Assert
        act.Should().NotThrow("MapEndpoint should successfully invoke the static Map method");
    }

    [Fact]
    public void Extensions_Should_Support_Fluent_Chaining()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        var result = app
            .MapEndpointGroups(Assembly.GetExecutingAssembly())
            .MapEndpoint<TestEndpoint>();

        // Assert
        result.Should().NotBeNull("fluent chaining should work correctly");
        result.Should().BeAssignableTo<IEndpointRouteBuilder>(
            "chained methods should return IEndpointRouteBuilder");
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Prefix_All_Routes()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly(), "api/v1");
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();
            var routeEndpoints = endpoints.OfType<RouteEndpoint>().ToList();

            routeEndpoints.Should().NotBeEmpty("endpoints should be registered with global prefix");
            routeEndpoints
                .Where(e => e.Metadata.OfType<IEndpointNameMetadata>().All(m => m.EndpointName != "HealthEndpoint"))
                .Should().AllSatisfy(e =>
                    e.RoutePattern.RawText.Should().StartWith("api/v1",
                        "all routes without [NoGlobalPrefix] should be prefixed with the global prefix"));
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithNullPrefix_Should_Not_Add_Prefix()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();
            var routeEndpoints = endpoints.OfType<RouteEndpoint>().ToList();

            routeEndpoints.Should().NotBeEmpty();
            routeEndpoints.Should().Contain(e =>
                    !e.RoutePattern.RawText!.StartsWith("api/v1"),
                "routes should not have a global prefix when none is provided");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Be_Idempotent()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly(), "api/v1");
        app.MapEndpointGroups(Assembly.GetExecutingAssembly(), "api/v1");
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints()
                .OfType<RouteEndpoint>()
                .Where(e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"))
                .ToList();

            endpoints.Should().HaveCount(1,
                "calling MapEndpointGroups multiple times with same prefix should not duplicate endpoints");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Not_Prefix_Groups_With_NoGlobalPrefixAttribute()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly(), "api/v1");
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints().OfType<RouteEndpoint>();

            var healthEndpoint = endpoints.Should().ContainSingle(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "HealthEndpoint"),
                "HealthEndpoint should be registered").Which;

            healthEndpoint.RoutePattern.RawText.Should().NotStartWith("api/v1",
                "groups with [NoGlobalPrefix] should ignore the global prefix");

            healthEndpoint.RoutePattern.RawText.Should().Be("/health",
                "route should remain as originally defined in the group");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_WithGlobalPrefix_Should_Still_Prefix_Groups_Without_NoGlobalPrefixAttribute()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly(), "api/v1");
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints().OfType<RouteEndpoint>();

            var testEndpoint = endpoints.Should().ContainSingle(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"),
                "TestEndpoint should be registered").Which;

            testEndpoint.RoutePattern.RawText.Should().StartWith("api/v1",
                "groups without [NoGlobalPrefix] should receive the global prefix");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }
}