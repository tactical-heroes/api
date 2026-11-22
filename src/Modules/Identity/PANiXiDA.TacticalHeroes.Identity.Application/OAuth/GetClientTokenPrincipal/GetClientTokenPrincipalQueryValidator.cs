namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalQueryValidator
    : AbstractValidator<GetClientTokenPrincipalQuery>
{
    public GetClientTokenPrincipalQueryValidator()
    {
        RuleFor(query => query.ClientId)
            .NotEmpty();
    }
}
