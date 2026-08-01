using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Domain.Abstractions;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class DependencyInjectionConventionTests
{
    private static readonly Type[] RepositoryDefinitions =
    [
        typeof(IRepository<,>),
        typeof(IReadRepository<>)
    ];

    [Fact(DisplayName = "Infrastructure implementations should be registered for application abstractions when declared")]
    public void InfrastructureImplementations_Should_BeRegisteredForApplicationAbstractions_When_Declared()
    {
        var registrations = GetAbstractionRegistrations();
        var violations = registrations
            .Where(registration => !registration.ServiceDescriptors.Any(
                descriptor => IsImplementation(
                    descriptor,
                    registration.Implementation)))
            .Select(registration =>
                $"{registration.Implementation.FullName} implements " +
                $"{registration.Abstraction.FullName} and must be registered " +
                $"for that abstraction in the module service collection.")
            .ToArray();

        Assert.NotEmpty(registrations);
        Assert.True(
            violations.Length == 0,
            $"Missing Infrastructure registrations for Application or " +
            $"Domain abstractions:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Repository implementations should be registered exactly once as scoped when declared")]
    public void RepositoryImplementations_Should_BeRegisteredExactlyOnceAsScoped_When_Declared()
    {
        var registrations = GetAbstractionRegistrations()
            .Where(registration => IsRepositoryContract(
                registration.Abstraction))
            .ToArray();
        var violations = registrations
            .Where(registration =>
                registration.ServiceDescriptors.Length != 1 ||
                registration.ServiceDescriptors[0].Lifetime !=
                ServiceLifetime.Scoped ||
                !IsImplementation(
                    registration.ServiceDescriptors[0],
                    registration.Implementation))
            .Select(registration =>
                $"{registration.Abstraction.FullName} must have exactly one " +
                $"scoped registration for " +
                $"{registration.Implementation.FullName}; found " +
                $"{FormatDescriptors(registration.ServiceDescriptors)}.")
            .ToArray();

        Assert.NotEmpty(registrations);
        Assert.True(
            violations.Length == 0,
            $"Repository registration violations:{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    [Fact(DisplayName = "Database contexts should be registered exactly once as scoped when declared")]
    public void DatabaseContexts_Should_BeRegisteredExactlyOnceAsScoped_When_Declared()
    {
        var registrations = GetDatabaseContextRegistrations();
        var violations = registrations
            .Where(registration =>
                registration.ServiceDescriptors.Length != 1 ||
                registration.ServiceDescriptors[0].Lifetime !=
                ServiceLifetime.Scoped)
            .Select(registration =>
                $"{registration.DatabaseContext.FullName} must have exactly " +
                $"one scoped registration; found " +
                $"{FormatDescriptors(registration.ServiceDescriptors)}.")
            .ToArray();

        Assert.NotEmpty(registrations);
        Assert.True(
            violations.Length == 0,
            $"Database context registration violations:" +
            $"{Environment.NewLine}" +
            string.Join(Environment.NewLine, violations));
    }

    private static AbstractionRegistration[] GetAbstractionRegistrations()
    {
        return
        [
            .. ArchitectureDefinition.Modules
                .SelectMany(module =>
                {
                    var domainAssembly = GetAssembly(
                        module.DomainAssemblyName);
                    var applicationAssembly = GetAssembly(
                        module.ApplicationAssemblyName);
                    var infrastructureAssembly = GetAssembly(
                        module.InfrastructureAssemblyName);
                    var serviceCollection =
                        InfrastructureServiceCollectionFactory.Create(
                            infrastructureAssembly);
                    var abstractions = domainAssembly
                        .GetTypes()
                        .Concat(applicationAssembly.GetTypes())
                        .Where(type => type.IsInterface)
                        .ToArray();

                    return infrastructureAssembly
                        .GetTypes()
                        .Where(type => type is
                            {
                                IsClass: true,
                                IsAbstract: false
                            })
                        .SelectMany(implementation => abstractions
                            .Where(abstraction =>
                                abstraction.IsAssignableFrom(implementation))
                            .Select(abstraction =>
                                new AbstractionRegistration(
                                    Abstraction: abstraction,
                                    Implementation: implementation,
                                    ServiceDescriptors:
                                    [
                                        .. serviceCollection.Where(
                                            descriptor =>
                                                descriptor.ServiceType ==
                                                abstraction)
                                    ])));
                })
                .OrderBy(
                    registration => registration.Abstraction.FullName,
                    StringComparer.Ordinal)
                .ThenBy(
                    registration => registration.Implementation.FullName,
                    StringComparer.Ordinal)
        ];
    }

    private static DatabaseContextRegistration[]
        GetDatabaseContextRegistrations()
    {
        return
        [
            .. ArchitectureDefinition.Modules
                .SelectMany(module =>
                {
                    var infrastructureAssembly = GetAssembly(
                        module.InfrastructureAssemblyName);
                    var serviceCollection =
                        InfrastructureServiceCollectionFactory.Create(
                            infrastructureAssembly);

                    return infrastructureAssembly
                        .GetTypes()
                        .Where(type =>
                            type is
                            {
                                IsClass: true,
                                IsAbstract: false
                            } &&
                            typeof(DbContext).IsAssignableFrom(type))
                        .Select(databaseContext =>
                            new DatabaseContextRegistration(
                                DatabaseContext: databaseContext,
                                ServiceDescriptors:
                                [
                                    .. serviceCollection.Where(descriptor =>
                                        descriptor.ServiceType ==
                                        databaseContext)
                                ]));
                })
                .OrderBy(
                    registration => registration.DatabaseContext.FullName,
                    StringComparer.Ordinal)
        ];
    }

    private static System.Reflection.Assembly GetAssembly(
        string assemblyName)
    {
        return ArchitectureDefinition.ProductionAssemblies.Single(
            assembly => string.Equals(
                assembly.GetName().Name,
                assemblyName,
                StringComparison.Ordinal));
    }

    private static bool IsRepositoryContract(Type abstraction)
    {
        return abstraction
            .GetInterfaces()
            .Append(abstraction)
            .Any(candidate =>
                candidate.IsGenericType &&
                RepositoryDefinitions.Contains(
                    candidate.GetGenericTypeDefinition()));
    }

    private static bool IsImplementation(
        ServiceDescriptor descriptor,
        Type implementation)
    {
        return descriptor.ImplementationType == implementation ||
               descriptor.ImplementationInstance?.GetType() == implementation;
    }

    private static string FormatDescriptors(
        IReadOnlyCollection<ServiceDescriptor> descriptors)
    {
        return descriptors.Count == 0
            ? "<none>"
            : string.Join(
                ", ",
                descriptors.Select(descriptor =>
                    $"{descriptor.Lifetime}:" +
                    $"{descriptor.ImplementationType?.FullName ?? "factory"}"));
    }

    private sealed record AbstractionRegistration(
        Type Abstraction,
        Type Implementation,
        ServiceDescriptor[] ServiceDescriptors);

    private sealed record DatabaseContextRegistration(
        Type DatabaseContext,
        ServiceDescriptor[] ServiceDescriptors);
}
