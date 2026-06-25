namespace Organization.Product.ArchitectureTests.Definitions;

internal sealed record ModuleArchitecture(
    string Name,
    string DomainAssemblyName,
    string ApplicationAssemblyName,
    string InfrastructureAssemblyName,
    string PresentationAssemblyName);
