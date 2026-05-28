using System.Reflection;
using System.Reflection.Emit;
using FluentAssertions;
using Microsoft.AspNetCore.Routing;
using ZedEndpoints.Abstractions;
using ZedEndpoints.Extensions;

namespace ZedEndpoints.Tests;

/// <summary>
/// Tests for endpoint group instantiation behavior in MapEndpointGroups.
/// </summary>
public sealed class EndpointGroupInstantiationTests
{
    [Fact]
    public void MapEndpointGroups_Should_Throw_When_EndpointGroup_Has_No_Public_Constructor()
    {
        // Arrange
        var assembly = CreateDynamicAssemblyWithEndpointGroup(publicConstructor: false);
        var app = TestHelpers.CreateTestBuilder().Build();

        // Act
        Action act = () => app.MapEndpointGroups(assembly: assembly);

        // Assert
        act.Should().Throw<MissingMethodException>(
            "endpoint groups with no public parameterless constructor cannot be instantiated " +
            "and should fail fast instead of being silently skipped");
    }

    [Fact]
    public void MapEndpointGroups_Should_Not_Throw_When_EndpointGroup_Has_Public_Constructor()
    {
        // Arrange
        var assembly = CreateDynamicAssemblyWithEndpointGroup(publicConstructor: true);
        var app = TestHelpers.CreateTestBuilder().Build();

        // Act
        Action act = () => app.MapEndpointGroups(assembly: assembly);

        // Assert
        act.Should().NotThrow(
            "endpoint groups with a public parameterless constructor should register without errors");
    }

    [Fact]
    public void MapEndpointGroups_Should_Include_TypeName_In_Exception_When_Cannot_Be_Instantiated()
    {
        // Arrange
        var assembly = CreateDynamicAssemblyWithEndpointGroup(publicConstructor: false);
        var app = TestHelpers.CreateTestBuilder().Build();

        // Act
        Action act = () => app.MapEndpointGroups(assembly: assembly);

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("*DynamicEndpointGroup*",
                "the error should identify the problematic endpoint group type by name");
    }

    private static Assembly CreateDynamicAssemblyWithEndpointGroup(bool publicConstructor)
    {
        var assemblyName = new AssemblyName("ZedEndpoints.Tests.Dynamic");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("Main");

        var typeBuilder = moduleBuilder.DefineType(
            "DynamicEndpointGroup",
            TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed,
            typeof(object),
            new[] { typeof(IEndpointGroup) });

        var mapGroupMethod = typeBuilder.DefineMethod(
            nameof(IEndpointGroup.MapGroup),
            MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
            typeof(void),
            new[] { typeof(IEndpointRouteBuilder) });
        var il = mapGroupMethod.GetILGenerator();
        il.Emit(OpCodes.Ret);
        typeBuilder.DefineMethodOverride(
            mapGroupMethod,
            typeof(IEndpointGroup).GetMethod(nameof(IEndpointGroup.MapGroup))!);

        var ctorAccess = publicConstructor ? MethodAttributes.Public : MethodAttributes.Private;
        var ctor = typeBuilder.DefineConstructor(ctorAccess, CallingConventions.Standard, Type.EmptyTypes);
        var ctorIl = ctor.GetILGenerator();
        ctorIl.Emit(OpCodes.Ldarg_0);
        ctorIl.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
        ctorIl.Emit(OpCodes.Ret);

        typeBuilder.CreateType();
        return assemblyBuilder;
    }
}
