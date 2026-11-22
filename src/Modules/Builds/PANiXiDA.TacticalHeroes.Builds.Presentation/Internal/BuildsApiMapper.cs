using PANiXiDA.TacticalHeroes.Modules.Builds.Application.Builds.GetInternalBuild;
using PANiXiDA.TacticalHeroes.Modules.Builds.Contracts.Builds;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.Internal;

[Mapper]
internal static partial class BuildsApiMapper
{
    internal static partial GetInternalBuildQuery ToQuery(Guid id);
    internal static partial BuildResponse ToResponse(BuildReadModel build);
}
