namespace PANiXiDA.TacticalHeroes.Identity.Application.OAuth.Read.ExchangeToken.GetClientTokenPrincipal;

public sealed class GetClientTokenPrincipalQueryValidator
    : AbstractValidator<GetClientTokenPrincipalQuery>
{
    public GetClientTokenPrincipalQueryValidator()
    {
        RuleFor(query => query.ClientId)
            .NotEmpty();
    }
}
