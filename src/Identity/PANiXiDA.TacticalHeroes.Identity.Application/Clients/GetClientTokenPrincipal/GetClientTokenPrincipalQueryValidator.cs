namespace PANiXiDA.TacticalHeroes.Identity.Application.Clients.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalQueryValidator
    : AbstractValidator<GetClientTokenPrincipalQuery>
{
    public GetClientTokenPrincipalQueryValidator()
    {
        RuleFor(query => query.ClientId)
            .NotEmpty();
    }
}
