using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Application.Builds.GetInternalBuild;

public sealed class GetInternalBuildQueryValidator : AbstractValidator<GetInternalBuildQuery>
{
    public GetInternalBuildQueryValidator()
    {
        RuleFor(query => query.Id).MustBeValidDomainValue(BuildId.Create);
    }
}
