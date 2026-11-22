namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Definitions;

internal sealed record ModuleArchitecture(
    string Name,
    string ContractsAssemblyName,
    string DomainAssemblyName,
    string ApplicationAssemblyName,
    string InfrastructureAssemblyName,
    string PresentationAssemblyName)
{
    internal string DomainNamespace => ArchitectureDefinition.GetModuleNamespace(DomainAssemblyName);

    internal string ApplicationNamespace => ArchitectureDefinition.GetModuleNamespace(ApplicationAssemblyName);

    internal string InfrastructureNamespace => ArchitectureDefinition.GetModuleNamespace(InfrastructureAssemblyName);

    internal string PresentationNamespace => ArchitectureDefinition.GetModuleNamespace(PresentationAssemblyName);
}
