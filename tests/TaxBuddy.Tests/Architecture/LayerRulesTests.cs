using System.Reflection;
using Microsoft.AspNetCore.Components;
using TaxBuddy.Web.Domain;

namespace TaxBuddy.Tests.Architecture;

/// <summary>
/// SDD dependency rules for the single web project (ADR 010): the compiler no longer separates the layers, so this test does.
/// It inspects member signatures and injected dependencies by reflection; it does not read method bodies, so review still
/// covers code that creates such types inside a method.
/// </summary>
public sealed class LayerRulesTests
{
    private static readonly Assembly Web = typeof(AccountantProfile).Assembly;

    private static readonly string[] ForbiddenInCoreLayers =
    [
        "Microsoft.AspNetCore",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "TaxBuddy.Web.Components",
        "TaxBuddy.Web.Data",
        "TaxBuddy.Web.Services",
    ];

    [Theory]
    [InlineData("TaxBuddy.Web.Domain")]
    [InlineData("TaxBuddy.Web.Application")]
    public void Domain_and_Application_use_no_web_or_database_types(string layer)
    {
        var violations = TypesIn(layer)
            .SelectMany(type => ReferencedTypes(type).Select(used => (type, used)))
            .Where(pair => ForbiddenInCoreLayers.Any(prefix => pair.used.FullName?.StartsWith(prefix, StringComparison.Ordinal) == true
                                                              || pair.used.Namespace?.StartsWith(prefix, StringComparison.Ordinal) == true))
            .Select(pair => $"{pair.type.FullName} uses {pair.used.FullName}")
            .Distinct()
            .ToList();

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Domain_does_not_depend_on_Application()
    {
        var violations = TypesIn("TaxBuddy.Web.Domain")
            .SelectMany(type => ReferencedTypes(type).Where(t => t.Namespace?.StartsWith("TaxBuddy.Web.Application", StringComparison.Ordinal) == true)
                .Select(used => $"{type.FullName} uses {used.FullName}"))
            .ToList();

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void Components_never_inject_the_database_or_data_layer()
    {
        var violations = Web.GetTypes()
            .Where(t => typeof(ComponentBase).IsAssignableFrom(t))
            .SelectMany(t => t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttribute<InjectAttribute>() is not null)
                .Where(p => p.PropertyType.Namespace?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true
                            || p.PropertyType.Namespace?.StartsWith("TaxBuddy.Web.Data", StringComparison.Ordinal) == true
                            || (p.PropertyType.IsGenericType && p.PropertyType.GetGenericArguments().Any(a => a.Namespace?.StartsWith("TaxBuddy.Web.Data", StringComparison.Ordinal) == true)))
                .Select(p => $"{t.FullName} injects {p.PropertyType.FullName}"))
            .ToList();

        Assert.True(violations.Count == 0, string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<Type> TypesIn(string layer) =>
        Web.GetTypes().Where(t => t.Namespace is { } ns && (ns == layer || ns.StartsWith(layer + ".", StringComparison.Ordinal)));

    private static IEnumerable<Type> ReferencedTypes(Type type)
    {
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        var direct = new List<Type?> { type.BaseType };
        direct.AddRange(type.GetInterfaces());
        direct.AddRange(type.GetFields(all).Select(f => f.FieldType));
        direct.AddRange(type.GetProperties(all).Select(p => p.PropertyType));
        foreach (var method in type.GetMethods(all).Cast<MethodBase>().Concat(type.GetConstructors(all)))
        {
            direct.AddRange(method.GetParameters().Select(p => p.ParameterType));
            if (method is MethodInfo info)
            {
                direct.Add(info.ReturnType);
            }
        }

        return direct.OfType<Type>().SelectMany(Expand).Distinct();
    }

    private static IEnumerable<Type> Expand(Type type)
    {
        if (type.HasElementType && type.GetElementType() is { } element)
        {
            foreach (var inner in Expand(element)) yield return inner;
            yield break;
        }

        yield return type;
        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments().SelectMany(Expand)) yield return argument;
        }
    }
}
