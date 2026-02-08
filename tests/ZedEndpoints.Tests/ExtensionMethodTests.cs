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
}