using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.DbContexts;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read;
using PANiXiDA.Core.Infrastructure.Persistence.Ef.Read.Models;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class AggregateReadPersistenceConventionTests
{
    [Fact(DisplayName = "Aggregate roots should have registered read persistence when declared")]
    public void AggregateRoots_Should_HaveRegisteredReadPersistence_When_Declared()
    {
        var modules = ArchitectureDefinition.s_modules;

        var violations = modules.SelectMany(GetViolations).ToArray();

        Assert.NotEmpty(modules.SelectMany(InfrastructurePersistenceConvention.GetAggregateRootTypes));
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetViolations(ModuleArchitecture module)
    {
        var aggregates = InfrastructurePersistenceConvention.GetAggregateRootTypes(module);
        if (aggregates.Length == 0)
        {
            yield break;
        }

        var application = ArchitectureDefinition.s_productionAssemblies
            .Single(assembly => assembly.GetName().Name == module.ApplicationAssemblyName);
        var infrastructure = ArchitectureDefinition.s_productionAssemblies
            .Single(assembly => assembly.GetName().Name == module.InfrastructureAssemblyName);
        var contextName = InfrastructurePersistenceConvention.GetModuleShortName(module) + "ReadDbContext";
        var contextType = infrastructure.GetType(module.InfrastructureAssemblyName + ".Persistence.Core." + contextName);
        if (contextType is null || contextType.IsAbstract ||
            InfrastructurePersistenceConvention.GetClosedGenericBaseType(contextType, typeof(ReadDbContext<>)) is null)
        {
            yield return $"{module.Name} has aggregates and must declare '{contextName}' in Persistence/Core.";
            yield break;
        }

        using var provider = InfrastructureServiceCollectionFactory.Create(infrastructure).BuildServiceProvider();
        using var scope = provider.CreateScope();
        if (scope.ServiceProvider.GetService(contextType) is not DbContext context)
        {
            yield return $"{contextType.FullName} must be registered in module services.";
            yield break;
        }

        foreach (var aggregate in aggregates)
        {
            var feature = EnglishNamingConvention.Pluralize(aggregate.Name);
            var contractName = $"{module.ApplicationAssemblyName}.{feature}.Abstractions.I{feature}ReadRepository";
            var contract = application.GetType(contractName);
            if (contract is not { IsInterface: true } ||
                InfrastructurePersistenceConvention.GetClosedGenericInterface(contract, typeof(IReadRepository<>)) is null)
            {
                yield return $"{aggregate.FullName} must declare Application interface '{contractName}'.";
            }

            var readNamespace = $"{module.InfrastructureAssemblyName}.Persistence.Features.{feature}.Read";
            var modelName = $"{readNamespace}.DbModels.{aggregate.Name}ReadDbModel";
            var model = infrastructure.GetType(modelName);
            if (model is null || model.IsAbstract ||
                InfrastructurePersistenceConvention.GetClosedGenericBaseType(model, typeof(ReadDbModel<>)) is null)
            {
                yield return $"{aggregate.FullName} must declare '{modelName}'.";
            }
            else
            {
                foreach (var violation in InfrastructurePersistenceConvention.GetLocationViolations(
                             model, "Persistence", "Features", feature, "Read", "DbModels"))
                {
                    yield return violation;
                }

                if (context.Model.FindEntityType(model) is null)
                {
                    yield return $"{model.FullName} must be mapped in '{contextType.FullName}'.";
                }
            }

            var repositoryName = $"{readNamespace}.{feature}ReadRepository";
            var repository = infrastructure.GetType(repositoryName);
            if (repository is null || repository.IsAbstract)
            {
                yield return $"{aggregate.FullName} must declare '{repositoryName}'.";
                continue;
            }

            foreach (var violation in InfrastructurePersistenceConvention.GetLocationViolations(
                         repository, "Persistence", "Features", feature, "Read"))
            {
                yield return violation;
            }

            var repositoryBase = InfrastructurePersistenceConvention.GetClosedGenericBaseType(
                repository, typeof(EfReadRepository<,,>));
            if (repositoryBase is null || repositoryBase.GenericTypeArguments[0] != contextType ||
                repositoryBase.GenericTypeArguments[2] != model)
            {
                yield return $"{repository.FullName} must use '{contextName}' and '{modelName}' through EfReadRepository.";
            }

            if (contract is not null &&
                (!contract.IsAssignableFrom(repository) || scope.ServiceProvider.GetService(contract)?.GetType() != repository))
            {
                yield return $"{repository.FullName} must implement and be registered as '{contract.FullName}'.";
            }
        }
    }
}
