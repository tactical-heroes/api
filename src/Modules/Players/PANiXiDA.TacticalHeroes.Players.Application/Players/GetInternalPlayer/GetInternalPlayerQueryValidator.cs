using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.GetInternalPlayer;

public sealed class GetInternalPlayerQueryValidator : AbstractValidator<GetInternalPlayerQuery>
{
    public GetInternalPlayerQueryValidator()
    {
        RuleFor(query => query.UserId).MustBeValidDomainValue(UserId.Create);
    }
}
