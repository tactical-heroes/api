using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

public sealed class GetInternalPlayerQueryValidator : AbstractValidator<GetInternalPlayerQuery>
{
    public GetInternalPlayerQueryValidator()
    {
        RuleFor(query => query.UserId).MustBeValidDomainValue(UserId.Create);
    }
}
