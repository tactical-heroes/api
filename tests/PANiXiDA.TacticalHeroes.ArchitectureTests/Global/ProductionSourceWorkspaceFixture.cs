using Microsoft.CodeAnalysis.MSBuild;

using PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

[assembly: AssemblyFixture(typeof(ProductionSourceWorkspaceFixture))]

namespace PANiXiDA.TacticalHeroes.ArchitectureTests.Global;

public sealed class ProductionSourceWorkspaceFixture : IDisposable
{
    internal static MSBuildWorkspace Workspace { get; } = MSBuildWorkspace.Create();

    public void Dispose()
    {
        Workspace.Dispose();
    }
}
