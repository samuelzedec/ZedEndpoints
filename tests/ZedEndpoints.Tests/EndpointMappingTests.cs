using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Extensions;
using ZedEndpoints.Tests.Fixtures;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for endpoint mapping functionality.
/// </summary>
public sealed class EndpointMappingTests
{
    [Fact]
    public async Task MapEndpoint_Should_Register_Endpoint()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpoint<TestEndpoint>();
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"),
                "endpoint should be registered");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public void MapEndpoint_Should_Support_Method_Chaining()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        var result = app
            .MapEndpoint<TestEndpoint>()
            .MapEndpoint<AnotherTestEndpoint>();

        // Assert
        result.Should().BeAssignableTo<IEndpointRouteBuilder>(
            "method should return IEndpointRouteBuilder for chaining");
    }

    [Fact]
    public async Task MapEndpoint_Should_Register_Multiple_Endpoints()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpoint<TestEndpoint>()
            .MapEndpoint<AnotherTestEndpoint>();

        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();

            endpoints.Should().HaveCountGreaterThanOrEqualTo(2,
                "multiple endpoints should be registered");

            endpoints.Should().Contain(e => e.Metadata.OfType<IEndpointNameMetadata>()
                .Any(m => m.EndpointName == "TestEndpoint"));

            endpoints.Should().Contain(e => e.Metadata.OfType<IEndpointNameMetadata>()
                .Any(m => m.EndpointName == "AnotherTestEndpoint"));
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpoint_Should_Work_With_RouteGroups()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        var group = app.MapGroup("api/v1");
        group.MapEndpoint<TestEndpoint>();

        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();
            var routeEndpoints = endpoints.OfType<RouteEndpoint>().ToList();

            routeEndpoints.Should().Contain(
                e => e.RoutePattern.RawText == "api/v1/test",
                "group prefix 'api/v1' should be applied to the endpoint route '/test'");

            routeEndpoints.Should().Contain(e => e.Metadata.OfType<IEndpointNameMetadata>()
                .Any(m => m.EndpointName == "TestEndpoint"));
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }
}