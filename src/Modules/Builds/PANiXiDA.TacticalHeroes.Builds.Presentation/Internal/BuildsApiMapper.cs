using PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;
using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Builds.Presentation.Internal;

[Mapper]
internal static partial class BuildsApiMapper
{
    internal static partial GetInternalBuildQuery ToQuery(Guid id);
    internal static partial BuildResponse ToResponse(BuildReadModel build);
}
