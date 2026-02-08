using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Tests.Fixtures;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for IEndpointGroup functionality.
/// </summary>
public sealed class EndpointGroupTests
{
    [Fact]
    public async Task EndpointGroup_Should_Register_Grouped_Endpoints()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();
        var group = new TestEndpointGroup();

        // Act
        group.MapGroup(app);
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "TestEndpoint"),
                "TestEndpoint should be registered within the group");

            endpoints.Should().Contain(
                e => e.Metadata.OfType<IEndpointNameMetadata>()
                    .Any(m => m.EndpointName == "AnotherTestEndpoint"),
                "AnotherTestEndpoint should be registered within the group");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public async Task EndpointGroup_Should_Apply_Group_Prefix()
    {
        // Arrange
        var builder = TestHelpers.CreateTestBuilder();
        var app = builder.Build();
        var group = new TestEndpointGroup();

        // Act
        group.MapGroup(app);
        await app.StartAsync();

        try
        {
            // Assert
            var endpoints = app.GetEndpoints();

            var routeEndpoints = endpoints
                .OfType<RouteEndpoint>()
                .ToList();

            routeEndpoints.Should().Contain(
                e => e.RoutePattern.RawText != null && e.RoutePattern.RawText.StartsWith("api/test"),
                "group prefix 'api/test' should be applied to all routes");

            // Verifica rotas específicas
            routeEndpoints.Should().Contain(
                e => e.RoutePattern.RawText == "api/test/test",
                "TestEndpoint should be at api/test/test");

            routeEndpoints.Should().Contain(
                e => e.RoutePattern.RawText == "api/test/another",
                "AnotherTestEndpoint should be at api/test/another");
        }
        finally
        {
            await app.DisposeTestsAsync();
        }
    }

    [Fact]
    public void EndpointGroup_Should_Create_Instance_With_Parameterless_Constructor()
    {
        // Arrange & Act
        var act = () => Activator.CreateInstance(typeof(TestEndpointGroup));

        // Assert
        act.Should().NotThrow("endpoint groups should have parameterless constructors");

        var instance = act();
        instance.Should().NotBeNull();
        instance.Should().BeOfType<TestEndpointGroup>();
        instance.Should().BeAssignableTo<IEndpointGroup>();
    }
}