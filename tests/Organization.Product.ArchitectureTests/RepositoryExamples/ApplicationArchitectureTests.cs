using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Organization.Product.ArchitectureTests.Application;

public sealed class ApplicationArchitectureTests
{
    [Fact(DisplayName = "Application handlers should be public")]
    public void ApplicationHandlers_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.ApplicationAssemblyNamePattern)
            .And().HaveNameEndingWith("Handler")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Application commands should be public")]
    public void ApplicationCommands_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.ApplicationAssemblyNamePattern)
            .And().HaveNameEndingWith("Command")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Application queries should be public")]
    public void ApplicationQueries_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.ApplicationAssemblyNamePattern)
            .And().HaveNameEndingWith("Query")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }

    [Fact(DisplayName = "Application read models should be public")]
    public void ApplicationReadModels_Should_Be_Public()
    {
        Classes().That()
            .ResideInAssemblyMatching(ArchitectureDefinition.ApplicationAssemblyNamePattern)
            .And().HaveNameEndingWith("ReadModel")
            .Should().BePublic()
            .Check(ArchitectureDefinition.Architecture);
    }
}
