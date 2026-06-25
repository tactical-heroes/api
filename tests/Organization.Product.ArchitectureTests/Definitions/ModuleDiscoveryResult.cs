namespace Organization.Product.ArchitectureTests.Definitions;

internal sealed record ModuleDiscoveryResult(
    IReadOnlyCollection<ModuleArchitecture> Modules,
    IReadOnlyCollection<string> Errors);
