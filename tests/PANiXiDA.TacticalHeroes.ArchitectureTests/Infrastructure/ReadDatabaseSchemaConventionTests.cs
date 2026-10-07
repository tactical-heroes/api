using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Infrastructure.Persistence.Ef.DbContexts;

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Infrastructure;

public sealed class ReadDatabaseSchemaConventionTests
{
    [Fact(DisplayName = "Read database models should match persisted table columns when declared")]
    public void ReadDatabaseModels_Should_MatchPersistedTableColumns_When_Declared()
    {
        var modules = ArchitectureDefinition.s_modules;

        var violations = modules.SelectMany(GetViolations).ToArray();

        Assert.NotEmpty(modules);
        Assert.True(violations.Length == 0, string.Join(Environment.NewLine, violations));
    }

    private static IEnumerable<string> GetViolations(ModuleArchitecture module)
    {
        var infrastructure = ArchitectureDefinition.s_productionAssemblies
            .Single(assembly => assembly.GetName().Name == module.InfrastructureAssemblyName);
        var contexts = infrastructure.GetTypes()
            .Where(type => !type.IsAbstract && typeof(DbContext).IsAssignableFrom(type))
            .ToArray();
        using var provider = InfrastructureServiceCollectionFactory.Create(infrastructure).BuildServiceProvider();
        using var scope = provider.CreateScope();
        var writeTables = contexts
            .Where(type => type.Name.EndsWith("WriteDbContext", StringComparison.Ordinal))
            .Select(type => (DbContext)scope.ServiceProvider.GetRequiredService(type))
            .SelectMany(context => context.Model.GetRelationalModel().Tables)
            .ToDictionary(table => (table.Schema, table.Name));
        var readContexts = contexts
            .Where(type => InfrastructurePersistenceConvention.GetClosedGenericBaseType(type, typeof(ReadDbContext<>)) is not null)
            .Select(type => (DbContext)scope.ServiceProvider.GetRequiredService(type));

        foreach (var context in readContexts)
        {
            foreach (var readTable in context.Model.GetRelationalModel().Tables)
            {
                var tableName = $"{readTable.Schema}.{readTable.Name}";
                if (!writeTables.TryGetValue((readTable.Schema, readTable.Name), out var writeTable))
                {
                    yield return $"{context.GetType().Name}: read table '{tableName}' has no matching write table.";
                    continue;
                }

                var expectedColumns = writeTable.Columns.ToDictionary(column => column.Name);
                var actualColumns = readTable.Columns.ToDictionary(column => column.Name);
                var columnNames = expectedColumns.Keys.Union(actualColumns.Keys)
                    .Where(name => name is not ("tableoid" or "xmin" or "cmin" or "xmax" or "cmax" or "ctid"))
                    .Order(StringComparer.Ordinal);
                foreach (var name in columnNames)
                {
                    if (!expectedColumns.TryGetValue(name, out var expected))
                    {
                        yield return $"{tableName}.{name}: read model maps a column absent from the write table.";
                    }
                    else if (!actualColumns.TryGetValue(name, out var actual))
                    {
                        yield return $"{tableName}.{name}: read model is missing a persisted column.";
                    }
                    else
                    {
                        if (actual.ProviderClrType != expected.ProviderClrType || actual.IsNullable != expected.IsNullable)
                        {
                            yield return $"{tableName}.{name}: expected {Describe(expected)}, read model has {Describe(actual)}.";
                        }

                        if (actual.PropertyMappings.Any(mapping => mapping.Property.IsShadowProperty()))
                        {
                            yield return $"{tableName}.{name}: read column must be represented by a CLR property.";
                        }
                    }
                }
            }
        }
    }

    private static string Describe(IColumn column)
    {
        return $"{column.ProviderClrType.Name}, nullable={column.IsNullable}";
    }
}
