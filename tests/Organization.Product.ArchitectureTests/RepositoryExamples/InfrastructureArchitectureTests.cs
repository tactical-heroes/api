using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Organization.Product.ArchitectureTests.Infrastructure;

public sealed class InfrastructureArchitectureTests
{
    [Fact(DisplayName = "Infrastructure repositories should be public")]
    public void InfrastructureRepositories_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.InfrastructureAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Persistence.Features"))
            .And().HaveNameEndingWith("Repository")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Infrastructure read database models should be public")]
    public void InfrastructureReadDatabaseModels_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.InfrastructureAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Persistence.Features"))
            .And().HaveNameEndingWith("ReadDbModel")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Infrastructure mappers should be internal")]
    public void InfrastructureMappers_Should_Be_Internal()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.InfrastructureAssemblyNamePattern)
            .And().ResideInNamespaceMatching(ArchitectureDefinition.IncludeNamespaceSegment("Persistence.Features"))
            .And().HaveNameEndingWith("Mapper")
            .Should().BeInternal()
            .Check(ArchitectureDefinition.Architecture);
    }
}
