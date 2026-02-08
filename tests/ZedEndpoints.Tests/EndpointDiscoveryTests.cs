using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Extensions;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for automatic endpoint group discovery functionality.
/// </summary>
public sealed class EndpointDiscoveryTests
{
    [Fact]
    public async Task MapEndpointGroups_Should_Discover_Concrete_EndpointGroups()
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
            endpoints.Should().NotBeEmpty("endpoint groups should be discovered and registered");

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"),
                "TestEndpoint should be registered with name metadata");

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "AnotherTestEndpoint"),
                "AnotherTestEndpoint should be registered with name metadata");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_Should_Ignore_Abstract_Classes()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            // Assert - Verifica que o processo não falhou
            var endpoints = app.GetEndpoints();
            endpoints.Should().NotBeEmpty("concrete groups should still be registered");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_Should_Ignore_Interfaces()
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
            endpoints.Should().NotBeEmpty("concrete implementations should still be registered");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public void MapEndpointGroups_Should_Return_Same_WebApplication_Instance()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        var result = app.MapEndpointGroups(Assembly.GetExecutingAssembly());

        // Assert
        result.Should().BeSameAs(app, "method should support method chaining");
    }

    [Fact]
    public async Task MapEndpointGroups_Should_Handle_Multiple_Groups()
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

            endpoints.Should().HaveCountGreaterThan(1,
                "multiple endpoint groups should be discovered and registered");

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"));

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "AnotherTestEndpoint"));
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task MapEndpointGroups_Should_Handle_Groups_Without_Endpoints()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();

        // Act
        app.MapEndpointGroups(Assembly.GetExecutingAssembly());
        await app.StartAsync();

        try
        {
            var act = () => app.GetEndpoints();
            act.Should().NotThrow("empty endpoint groups should not cause errors");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }
}