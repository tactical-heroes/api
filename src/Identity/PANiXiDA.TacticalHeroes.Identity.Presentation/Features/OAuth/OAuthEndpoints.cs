using Asp.Versioning;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.OAuth;

internal sealed class OAuthEndpoints : IEndpointGroup
{
    public string Route { get; } = "connect";
    public string Name { get; } = "OAuth";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup($"/{Route}")
            .WithTags(Name);

        EndpointMapper.MapGroupEndpoints<OAuthEndpoints>(
            group,
            endpoints.ServiceProvider);
    }
}
