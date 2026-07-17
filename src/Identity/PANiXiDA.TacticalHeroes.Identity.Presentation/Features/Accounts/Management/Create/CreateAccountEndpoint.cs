using Microsoft.AspNetCore.Http;

using PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.GetDetails;

namespace PANiXiDA.TacticalHeroes.Identity.Presentation.Features.Accounts.Management.Create;

internal sealed class CreateAccountEndpoint : IEndpoint<AccountManagementEndpoints>
{
    public string Route { get; } = "/";
    public string Name { get; } = "CreateAccount";
    public string Summary { get; } = "Create account";

    public void Map(EndpointMapBuilder builder)
    {
        builder.MapPost(Handle)
            .Produces<CreateAccountResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> Handle(
        CreateAccountRequest request,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.SendAsync(
            CreateAccountMapper.ToCommand(request),
            cancellationToken);

        return result.ToHttpResult(id =>
            TypedResults.CreatedAtRoute(
                CreateAccountMapper.ToResponse(id),
                new GetAccountDetailsEndpoint().Name,
                new { id }));
    }
}
