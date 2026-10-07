using Asp.Versioning;

using Microsoft.AspNetCore.Routing;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files;

internal sealed class FilesEndpoints : IEndpointGroup
{
    internal const string IdRoute = "/{id:guid}";

    public string Route { get; } = "files";
    public string Name { get; } = "Files";
    public ApiVersion ApiVersion { get; } = new(majorVersion: 1, minorVersion: 0);

    public void Map(IEndpointRouteBuilder endpoints)
    {
        EndpointMapper
            .MapGroupEndpoints<FilesEndpoints>(endpoints)
            .RequireAuthorization();
    }
}
